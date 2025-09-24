using Microsoft.Data.Sqlite;

namespace GlutenFree.OddJob.Manager.Blazor;

public class sqlitehelperclass
{
    public static SqliteConnection Connection;

    public static string EnsureConnecttionStringExists(string connectionString)
    {
        if (Connection == null)
        {
            Connection = new SqliteConnection(connectionString);
            Connection.Open();
        }
        return connectionString;
    }
}