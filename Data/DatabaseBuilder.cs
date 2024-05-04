using Dapper;
using FeudingFamily.Models;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Text.Json;

namespace FeudingFamily.Data;

// this sucks, but i only need to use it once soooooo.
public class DatabaseBuilder
{

    private readonly IDbConnection _connection;
    public DatabaseBuilder(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task CreateTableAsync(string createTableSql)
    {
        try
        {
            var result = await _connection.ExecuteAsync(createTableSql);
            if (result == 0)
            {
                Console.WriteLine("$Result is {result} but no error thrown, table probably already exists");
            }

            Console.WriteLine($"Table created successfully. Result: {result}");
        }
        catch (SqliteException ex)
        {
            Console.WriteLine($"Error creating table. Exception: {ex.Message}");
            throw;
        }

    }

    public async Task PopulateTablesAsync(string jsonFilePath)
    {
        try
        {
            var questions = ReadAndParseJsonFile(jsonFilePath);

            if (questions is null)
            {
                return;
            }

            foreach (var question in questions)
            {
                if (CheckIfQuestionInDatabase(question))
                {
                    Console.WriteLine("Question already in DB");
                    continue;
                }


                var questionSql = @"
                    INSERT INTO Questions (Content) VALUES (@Content);
                ";

                var effectedQuestionRows = await _connection.ExecuteAsync(questionSql, new { Content = question.Content });

                if (effectedQuestionRows == 1)
                {
                    Console.WriteLine($"success for {question.Content}");
                }
                else
                {
                    Console.WriteLine($"\n\nFAIL!!!\n{question.Content}, {effectedQuestionRows}");
                }


                var lastInsertedRowId = await _connection.ExecuteScalarAsync("SELECT last_insert_rowid()");

                var answerSql = @"
                    INSERT INTO Answers (Content, Points, Ranking, QuestionId)
                    VALUES (@Content, @Points, @Ranking, @QuestionId);
                ";

                foreach (var answer in question.Answers)
                {
                    var parameters = new
                    {
                        Content = answer.Content,
                        Points = answer.Points,
                        Ranking = answer.Ranking,
                        QuestionId = lastInsertedRowId
                    };

                    var effectedAnswerRows = await _connection.ExecuteAsync(answerSql, parameters);
                    Console.WriteLine(effectedAnswerRows);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error populating tables. Exception: {ex}");
            throw;
        }


    }

    public bool CheckIfQuestionInDatabase(QuestionDto question)
    {
        var sql = @"
            SELECT COUNT(*) FROM Questions
            WHERE Content == @Content
        ";

        var rowsReturned = _connection.ExecuteScalar<int>(sql, new { question.Content });

        return rowsReturned != 0;

    }
    public List<QuestionDto>? ReadAndParseJsonFile(string jsonFilePath)
    {
        using var json = new StreamReader(jsonFilePath).BaseStream;

        try
        {
            json.Seek(0, SeekOrigin.Begin);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var jsonQuestions = JsonSerializer.Deserialize<List<JsonQuestionModel>>(json, options);

            if (jsonQuestions is null)
            {
                throw new Exception("JsonQuestions is null");
            }

            var questions = jsonQuestions.Select(q =>
                new QuestionDto
                {
                    Content = q.Question,
                    Answers =
                    [
                        new() {
                            Content = q.Answer1?.Text ?? string.Empty,
                            Points = q.Answer1?.Points ?? 0,
                            Ranking = 1
                        },
                        new() {
                            Content = q.Answer2?.Text ?? string.Empty,
                            Points = q.Answer2?.Points ?? 0,
                            Ranking = 2
                        },
                        new() {
                            Content = q.Answer3?.Text ?? string.Empty,
                            Points = q.Answer3?.Points ?? 0,
                            Ranking = 3
                        },
                        new() {
                            Content = q.Answer4?.Text ?? string.Empty,
                            Points = q.Answer4?.Points ?? 0,
                            Ranking = 4
                        },
                        new() {
                            Content = q.Answer5?.Text ?? string.Empty,
                            Points = q.Answer5?.Points ?? 0,
                            Ranking = 5
                        },
                    ]
                }
            ).ToList();

            return questions;
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: " + ex);
            return default;
        }

    }

    public async Task TestAsync()
    {
        await PopulateTablesAsync("Data/dbo/JsonQuestions/ff_questions.json");
    }

}