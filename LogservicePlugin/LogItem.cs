using System;
using System.Globalization;
using System.Text;

namespace LogservicePlugin
{
    public class LogItem
    {
        public int Priority { get; set; }
        public string Msg { get; set; }
        public string Hostname { get; set; }
        public string Servicename { get; set; }
        public uint Type { get; set; }
        public DateTime Data { get; set; }
        public string Date
        {
            get => Data.ToString(CultureInfo.InvariantCulture.DateTimeFormat.UniversalSortableDateTimePattern);
        }
        public string Base64Msg
        {
            get => Convert.ToBase64String(Encoding.Unicode.GetBytes(Msg));
        }
        public string LowerMsg
        {
            get => Msg.ToLower();
        }

        public LogItem(int priority, string msg, string hostname, string servicename, uint type)
        {
            Priority = priority;
            Msg = msg;
            Hostname = hostname;
            Servicename = servicename;
            Type = type;
            Data = DateTime.Now;
        }
    }
}
