using System;
using System.Text;
using System.Text.RegularExpressions;

namespace LogservicePlugin
{
    public static class Utils
    {
        public static (int, string, int, string) ParseChat(string str)
        {
            Regex r = new Regex(@"(\S+)=(\S+)");
            MatchCollection m = r.Matches(str);
            return (m[0].Groups[2].Value.ToInt32(), m[1].Groups[1].Value, m[1].Groups[2].Value.ToInt32(), m[2].Groups[0].Value.Replace("msg=", ""));
        }

        public static bool IsChat(string str)
        {
            return (str.Contains("Whisper:") && str.Contains("dst=")) ||
                (str.Contains("Chat:") && str.Contains("chl=")) ||
                (str.Contains("Guild:") && str.Contains("fid=")) ? true : false;
        }

        public static Services Cast(string service)
        {
            switch (service.ToLower())
            {
                case "gacd":
                    return Services.Gacd;
                case "gamed":
                    return Services.Gamed;
                case "gamedbd":
                    return Services.Gamedbd;
                case "gdeliveryd":
                    return Services.Gdeliveryd;
                case "uniquenamed":
                    return Services.Uniquenamed;
                case "gfaction":
                    return Services.Gfaction;
                case string s when service.ToLower().Contains("glinkd"):
                    return Services.Glinkd;
                case "chat":
                    return Services.Chat;
                default:
                    Console.WriteLine($"Unknown service: {service}");
                    return Services.Nan;
            }
        }

        public static string Base64Decode(string str) => Encoding.Unicode.GetString(Convert.FromBase64String(str));

        public static int ToInt32(this string str) => int.Parse(str);
    }
}
