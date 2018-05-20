using MySql.Data.MySqlClient;

namespace LogservicePlugin.Interfaces
{
    public interface IDatabase
    {
        MySqlConnection Database { get; set; }
    }
}
