using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic;

namespace FeudingFamily.Data;

public class DatabaseConnection : IDatabaseConnection
{
    private readonly SqliteConnection _connection;
    public SqliteConnection Connection
    {
        get
        {
            return _connection;
        }
    }
    public DatabaseConnection(string connectionString)
    {

        _connection = new SqliteConnection(connectionString);

    }

}

public interface IDatabaseConnection
{
    public SqliteConnection Connection { get; }
}