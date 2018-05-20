using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogservicePlugin;
using LogservicePlugin.Interfaces;
using MadMilkman.Ini;
using MySql.Data.MySqlClient;

namespace DemoPlugin
{
    public class Demo : IPlugin, ISettings, IDatabase
    {
        public string Name => "DemoPlugin";
        public string Version => "1.0";
        public string Author => "Kn1fe";

        public string BaseDirectory { get; set; }

        public Services ReqServices { get => Services.Chat | Services.Gamed; }

        public IniFile Ini { get; set; }

        public MySqlConnection Database { get; set; }

        public void Initialize()
        {
            Console.WriteLine("Reading settings");
        }

        public void Execute(LogItem item)
        {
            Console.WriteLine($"Input log: {item.Msg}");
        }
    }
}
