using MySqlConnector;

namespace AppMobile.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService()
    {
        _connectionString = new MySqlConnectionStringBuilder
        {
            Server = "mysql-absencemanagement.alwaysdata.net",
            Port = 3306,
            Database = "absencemanagement_bdd",
            UserID = "absencemanagement",
            Password = "pohJyr-9guqfo-hewpuw",
            SslMode = MySqlSslMode.Required,
            AllowPublicKeyRetrieval = true
        }.ConnectionString;
    }

    public MySqlConnection GetConnection() => new(_connectionString);
}
