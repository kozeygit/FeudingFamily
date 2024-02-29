namespace FeudingFamily.dbo.Tables;

public class CreateTableSql
{
    public static string Questions
    {
        get
        {
            return @"CREATE TABLE IF NOT EXISTS Questions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Content TEXT NOT NULL
                );";

        }

    }

    public static string Answers
    {
        get
        {
            return @"CREATE TABLE IF NOT EXISTS Answers (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Content TEXT NOT NULL,
                    Points INTEGER,
                    Ranking INTEGER,
                    QuestionId INTEGER,
                    FOREIGN KEY (QuestionId) REFERENCES Questions(Id)
                );";
        }
    }
}
