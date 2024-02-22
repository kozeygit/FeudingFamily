using FeudingFamily.Models;
using Microsoft.Data.Sqlite;
using System.Text.Json;
using Dapper;

namespace FeudingFamily.Data;

public class DatabaseBuilder
{

    public DatabaseBuilder(SqliteConnection sqliteConnection)
    {
        
    }
    private readonly string connectionString = $"Data Source=Data/FamilyFeudDB.db;Foreign Keys=true;";
    public void CreateQuestionsTable()
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            if (!TableExists(connection, "Questions"))
            {
                using (SqliteCommand command = connection.CreateCommand())
                {
                    string createTableQuery = @"CREATE TABLE Questions (
                                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                                Content TEXT NOT NULL
                                                );";

                    command.CommandText = createTableQuery;
                    command.ExecuteNonQuery();

                    Console.WriteLine("Questions table created successfully!");
                }
            }
            else
            {
                Console.WriteLine("Questions table already exists!");
            }

            connection.Close();
        }
    }

    public void CreateAnswersTable()
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            if (!TableExists(connection, "Answers"))
            {
                using (SqliteCommand command = connection.CreateCommand())
                {
                    string createTableQuery = @"CREATE TABLE Answers (
                                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                                Content TEXT NOT NULL,
                                                Points INTEGER,
                                                Ranking INTEGER,
                                                QuestionId INTEGER,
                                                FOREIGN KEY (QuestionId) REFERENCES Questions(Id)
                                                );";

                    command.CommandText = createTableQuery;
                    command.ExecuteNonQuery();

                    Console.WriteLine("Answers table created successfully!");
                }
            }
            else
            {
                Console.WriteLine("Answers table already exists!");
            }

            connection.Close();
        }
    }

    public bool TableExists(SqliteConnection connection, string tableName)
    {
        using (SqliteCommand command = connection.CreateCommand())
        {
            command.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}';";
            return command.ExecuteScalar() != null;
        }
    }

    public void PopulateDB(string jsonFilePath)
    {
        var questions = ReadAndParseJsonFile(jsonFilePath);

        // Console.WriteLine(questions[0]);
        // questions[0].Answers.ForEach(a => Console.WriteLine(a));

        foreach (var question in questions)
        {
            // Check if exact copy of question is in db already
            if (CheckIfQuestionInDatabase(question))
            {
                Console.WriteLine("Question already in DB");
                continue;
            }

            using var connection = new SqliteConnection(connectionString);
            
            // Insert Question
            var questionSql = @"INSERT INTO Questions (Content) VALUES (@Content);";

            var affectedRows = connection.Execute(questionSql, new { Content = question.Content });
            if (affectedRows == 1)
            {
                Console.WriteLine($"success for {question.Content}");
            }
            else
            {
                Console.WriteLine($"\n\nFAIL!!!\n{question.Content}, {affectedRows}");
            }

            // Insert Answers with last id as question id
            var lastInsertRowId = connection.ExecuteScalar("SELECT last_insert_rowid()");
            var answerSql = @"
                INSERT INTO Answers (Content, Points, Ranking, QuestionId)
                VALUES (@Content, @Points, @Ranking, @QuestionId);";

            foreach (var answer in question.Answers)
            {
                var dynamicParams = new DynamicParameters(answer);
                dynamicParams.Add("@QuestionId", lastInsertRowId);
                var affectedAnswerRows = connection.Execute(answerSql, dynamicParams);
                Console.WriteLine(affectedAnswerRows);
            }


        }

    }

    public bool CheckIfQuestionInDatabase(QuestionDto question)
    {
        using var connection = new SqliteConnection(connectionString);
        var sql = @"
        SELECT COUNT(*) FROM Questions
        WHERE Content == @Content";

        var rowsReturned = connection.ExecuteScalar<int>(sql, new { question.Content });
        if (rowsReturned == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    public List<QuestionDto> ReadAndParseJsonFile(string jsonFilePath)
    {
        using Stream json = new StreamReader(jsonFilePath).BaseStream;

        try
        {
            json.Seek(0, SeekOrigin.Begin);

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            List<JsonQuestionModel>? jsonQuestions = JsonSerializer.Deserialize<List<JsonQuestionModel>>(json, options);

            if (jsonQuestions is null)
            {
                Console.WriteLine("IT WAS NULL AHHHHHH");
                throw new Exception();
            }

            List<QuestionDto> questions = jsonQuestions.Select(q =>
                new QuestionDto
                {
                    Content = q.Question,
                    Answers = [
                        new AnswerDto
                        {
                            Content = q.Answer1.Text,
                            Points = q.Answer1.Points,
                            Ranking = 1
                        },
                        new AnswerDto
                        {
                            Content = q.Answer2.Text,
                            Points = q.Answer2.Points,
                            Ranking = 2
                        },
                        new AnswerDto
                        {
                            Content = q.Answer3.Text,
                            Points = q.Answer3.Points,
                            Ranking = 3
                        },
                        new AnswerDto
                        {
                            Content = q.Answer4.Text,
                            Points = q.Answer4.Points,
                            Ranking = 4
                        },
                        new AnswerDto
                        {
                            Content = q.Answer5.Text,
                            Points = q.Answer5.Points,
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
            return null;
        }

    }

    public void Test()
    {
        PopulateDB("Data/ff_questions.json");
    }

}