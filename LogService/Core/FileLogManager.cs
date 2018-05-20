using LogservicePlugin;
using LogservicePlugin.Interfaces;
using System;

namespace LogService.Core
{
    class FileLogManager : ILogManager
    {
        private Logger Log { get; set; }
        public bool Decode { get; set; }

        public FileLogManager(string name)
        {
            Log = new Logger(Settings.LogDir, name);
        }

        public void Process(object param)
        {
            LogItem item = param as LogItem;
            Log.Write($"[{Enum.GetName(typeof(LogType), item.Type)} Priority: {item.Priority}] {item.Msg}");
        }

        public void ProcessChat(object param)
        {
            LogItem item = param as LogItem;
            (int src, string type, int value, string msg) = Utils.ParseChat(item.Msg);
            if (Decode)
                item.Msg = item.Msg.Replace(msg, Utils.Base64Decode(msg));
            Log.Write($"[{Enum.GetName(typeof(LogType), item.Type)} Priority: {item.Priority}] {item.Msg}");
        }
    }
}
