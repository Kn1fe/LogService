using LogservicePlugin;
using LogservicePlugin.Interfaces;
using MySql.Data.MySqlClient;

namespace LogService.Core
{
    class MysqlLogManager : ILogManager, IDatabase
    {
        public MySqlConnection Database { get; set; }
        public bool Decode { get; set; }

        public void Process(object param)
        {
            LogItem item = param as LogItem;
            MySqlCommand dbcmd = Database.CreateCommand();
            dbcmd.CommandText = $"INSERT INTO {item.Servicename} (date, type, priority, msg) VALUES" +
                $" ('{item.Date}','{item.Type}', '{item.Priority}', '{item.Base64Msg}')";
            dbcmd.ExecuteNonQuery();
        }

        public void ProcessChat(object param)
        {
            LogItem item = param as LogItem;
            MySqlCommand dbcmd = Database.CreateCommand();
            (int src, string type, int value, string msg) = Utils.ParseChat(item.Msg);
            if (Decode)
                item.Msg = item.Msg.Replace(msg, Utils.Base64Decode(msg));
            dbcmd.CommandText = $"INSERT INTO chat (date, src, chl, msg) VALUES ('{item.Date}', '{src}', '{value}', '{msg}')";
            dbcmd.ExecuteNonQuery();
        }
    }
}
