using LogService.Core;
using LogservicePlugin;
using System;
using System.ServiceProcess;

namespace LogService
{
    class Program
    {
        public static Logger Logger { get; set; }

        static void Main(string[] args)
        {
            try
            {
                Settings.Init();
                Logger = new Logger(Settings.LogDir, "LogService");
                ServiceBase.Run(new LogserviceServer());
                //new LogserviceServer();
                //Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine();
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
