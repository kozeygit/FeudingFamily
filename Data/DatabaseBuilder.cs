using System.Data.Common;
using System.Runtime.CompilerServices;
using BlazorServer.Models;
using Microsoft.Data.Sqlite;
using System.Text.Json.Nodes;
using System.Text.Json;
using Microsoft.VisualBasic;

namespace BlazorServer.Data;

static public class DatabaseBuilder
{
    private static readonly string connectionString = $"Data Source=Data/FamilyFeudDB.db;foreign keys=true;";
    public static void CreateQuestionsTable()
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

    public static void CreateAnswersTable()
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

    public static bool TableExists(SqliteConnection connection, string tableName)
    {
        using (SqliteCommand command = connection.CreateCommand())
        {
            command.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}';";
            return command.ExecuteScalar() != null;
        }
    }

    public static void Populate(string jsonFilePath)
    {
        
    }

    public static List<JsonQuestionModel> ReadAndParseJsonFile(string jsonFilePath)
    {
        using Stream json = new StreamReader(jsonFilePath).BaseStream;
        
        try
        {
            json.Seek(0, SeekOrigin.Begin);

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            List<JsonQuestionModel>? questions = JsonSerializer.Deserialize<List<JsonQuestionModel>>(json, options);

            if (questions is null)
            {
                Console.WriteLine("IT WAS NULL AHHHHHH");
                throw new Exception();
            }         

            foreach (var q in questions)
            {
                Console.WriteLine(q.Question);
                Console.WriteLine(q.Answer1.Text);
                Console.WriteLine(q.Answer2.Text);
                Console.WriteLine(q.Answer3.Text);
                Console.WriteLine(q.Answer4.Text);
                Console.WriteLine(q.Answer5.Text);
            }
            return questions;
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("ERROR: " + ex);
            Console.WriteLine();
            Console.WriteLine();
            return null;
        }
        
    }

    public static void Test()
    {
        var _ = ReadAndParseJsonFile("Data/ff_questions.json");
    }

}