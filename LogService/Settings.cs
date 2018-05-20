using MadMilkman.Ini;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LogService
{
    public static class Settings
    {
        public static string MysqlHost { get; set; }
        public static string MysqlUser { get; set; }
        public static string MysqlPass { get; set; }
        public static string MysqlDB { get; set; }

        public static string TCPHost { get; set; }
        public static int TCPPort { get; set; }

        public static string UDPHost { get; set; }
        public static int UDPPort { get; set; }

        public static string LogDir { get; set; }

        public static List<(string, string, bool)> services = new List<(string, string, bool)>();

        public static bool Init()
        {
            try
            {
                IniFile ini = new IniFile();
                ini.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.ini"));

                TCPHost = ini.Sections["General"].Keys["TCPAdress"].Value;
                TCPPort = ini.Sections["General"].Keys["TCPPort"].Value.ToInt32();
                UDPHost = ini.Sections["General"].Keys["UDPAdress"].Value;
                UDPPort = ini.Sections["General"].Keys["UDPPort"].Value.ToInt32();
                LogDir = ini.Sections["General"].Keys["Dir"].Value;

                MysqlHost = ini.Sections["Mysql"].Keys["Host"].Value;
                MysqlUser = ini.Sections["Mysql"].Keys["User"].Value;
                MysqlPass = ini.Sections["Mysql"].Keys["Pass"].Value;
                MysqlDB = ini.Sections["Mysql"].Keys["Database"].Value;

                ini.Sections.Where(x => x.Name.StartsWith("Service")).ToList().ForEach(x =>
                {
                    string name = x.Keys["Name"].Value;
                    string type = x.Keys["Type"].Value;
                    bool decode = x.Keys["Decode"]?.Value.ToInt32() == 1 ? true : false;
                    services.Add((name, type, decode));
                });

                Console.WriteLine("Load settings complite!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while reading settings file {ex.Message}");
                return false;
            }
}

        public static byte ToByte(this string val) => byte.Parse(val);

        public static int ToInt32(this string val) => int.Parse(val);
    }
}
