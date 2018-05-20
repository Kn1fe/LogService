using LogservicePlugin;
using LogservicePlugin.Interfaces;
using MadMilkman.Ini;
using System;
using System.Collections.Generic;
using System.Net;

namespace QuestsGold
{
    public class QuestsGold : IPlugin, ISettings, ILogger
    {
        public string Name => "QuestsGold";
        public string Version => "1.0";
        public string Author => "Kn1fe";
        public string BaseDirectory { get; set; }
        public Services ReqServices { get => Services.Gamed; }
        public IniFile Ini { get; set; }
        public Logger Logger { get; set; }

        private Dictionary<int, int> Quests = new Dictionary<int, int>();

        public void Initialize()
        {
            foreach (var data in Ini.Sections["Quests"].Keys)
            {
                Quests.Add(int.Parse(data.Name), int.Parse(data.Value));
            }
        }

        public void Execute(LogItem item)
        {
            if (item.LowerMsg.Contains("deliverby"))
            {
                string[] parse = item.Msg.Split(':');
                int roleid = int.Parse(parse[2].Split('=')[1]);
                int taskid = Convert.ToInt32(parse[3].Split('=')[1]);
                if (Quests.ContainsKey(taskid))
                {
                    OctetsStream getroleid = new OctetsStream()
                    {
                        OpCode = 3412
                    };
                    getroleid.WriteInt32(-1);
                    getroleid.WriteInt32(roleid);
                    OctetsStream stream = new OctetsStream(getroleid.Send(new IPEndPoint(IPAddress.Loopback, 29400), true, false), RemoveHeaderType.Auto);
                    int userid = stream.ReadInt32();
                    stream = new OctetsStream()
                    {
                        OpCode = 521
                    };
                    stream.WriteInt32(userid);
                    stream.WriteInt32(Quests[taskid]);
                    stream.Send(new IPEndPoint(IPAddress.Loopback, 29400), false, false);
                    Logger.Write($"Userid: {userid}, Roleid: {roleid}, silver {Quests[taskid]}");
                }
            }
        }
    }
}
