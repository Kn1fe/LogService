using LogService.SocketServer;
using LogservicePlugin;
using LogservicePlugin.Interfaces;
using MadMilkman.Ini;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Timers;

namespace LogService.Core
{
    public class LogserviceServer : ServiceBase, IDatabase
    {
        private TcpServer tcpServer;
        private UdpServer udpServer;
        private System.Timers.Timer t = new System.Timers.Timer(10000);
        Dictionary<Services, ILogManager> manager = new Dictionary<Services, ILogManager>();
        List<IPlugin> plugins = new List<IPlugin>();

        public MySqlConnection Database { get; set; }

        public LogserviceServer()
        {
            //Connect to mysql
            try
            {
                Database = new MySqlConnection($"Server={Settings.MysqlHost};Database={Settings.MysqlDB};User ID={Settings.MysqlUser};Password={Settings.MysqlPass};Pooling=false");
                Database.Open();
            }
            catch (Exception ex)
            {
                Program.Logger.Write($"Can't connect to mysql server {ex.Message}");
            }
            //Connection checker
            t.Elapsed += CheckMysql;
            if (Database.State == System.Data.ConnectionState.Open)
                t.Start();
            else
                Program.Logger.Write($"Mysql connection not opened, skip checking");
            //Plugins loader
            string[] folders = Directory.GetDirectories(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins"));
            foreach (string plugin in folders)
            {
                string dll = Directory.GetFiles(plugin, "*.dll", SearchOption.TopDirectoryOnly).First();
                Type pluginType = Assembly.LoadFrom(dll).GetTypes().FirstOrDefault(t => typeof(IPlugin).IsAssignableFrom(t));
                IPlugin instance = Activator.CreateInstance(pluginType, null) as IPlugin;
                instance.BaseDirectory = plugin;
                if (instance.GetType().GetInterfaces().Contains(typeof(IDatabase)))
                {
                    (instance as IDatabase).Database = Database;
                }
                if (instance.GetType().GetInterfaces().Contains(typeof(ISettings)))
                {
                    (instance as ISettings).Ini = new IniFile();
                    (instance as ISettings).Ini.Load(Path.Combine(instance.BaseDirectory, "Settings.ini"));
                }
                if (instance.GetType().GetInterfaces().Contains(typeof(ILogger)))
                {
                    (instance as ILogger).Logger = new Logger(Path.Combine(Settings.LogDir, "Plugins"), instance.Name);
                }
                instance.Initialize();
                plugins.Add(instance);
                Console.WriteLine($"Plugin {instance.Name} version {instance.Version} by {instance.Author} has been loaded.");
            }
            //Services from settings
            //1 - name, 2 - type, 3 - decode
            Settings.services.ForEach(x =>
            {
                Services service = Utils.Cast(x.Item1);
                ILogManager logManager;
                if (x.Item2 == "mysql")
                {
                    logManager = new MysqlLogManager();
                }
                else
                {
                    logManager = new FileLogManager(Enum.GetName(typeof(Services), service));
                }
                logManager.Decode = x.Item3;
                manager.Add(service, logManager);
            });
            //tcpServer = new TcpServer(Settings.TCPPort);
            //udpServer = new UdpServer(IPAddress.Parse(Settings.UDPHost), Settings.UDPPort);
            //tcpServer.SocketAccepted += TcpServer_SocketAccepted;
            //udpServer.Received += UdpServer_Received;
            //tcpServer.Start(IPAddress.Parse(Settings.TCPHost));
        }

        private void CheckMysql(object sender, ElapsedEventArgs e)
        {
            if (Database.State != System.Data.ConnectionState.Open)
            {
                Program.Logger.Write($"Mysql is not connected, trying reconnect");
                try
                {
                    Database.Open();
                }
                catch
                {
                    Program.Logger.Write($"Reconnect failure");
                }
            }
        }

        protected override void OnStart(string[] args)
        {
            tcpServer = new TcpServer(Settings.TCPPort);
            udpServer = new UdpServer(IPAddress.Parse(Settings.UDPHost), Settings.UDPPort);
            tcpServer.SocketAccepted += TcpServer_SocketAccepted;
            udpServer.Received += UdpServer_Received;
            tcpServer.Start(IPAddress.Parse(Settings.TCPHost));
            base.OnStart(args);
        }

        #region Tcp client accept
        private void TcpServer_SocketAccepted(object sender, SocketAcceptedEventHandler e)
        {
            Program.Logger.Write("Add tcp connection");
            Client client = new Client(e.acceptedSocket);
            client.Received += new Client.ClientReceivedHandler(TcpServer_ClientReceived);
            client.Disconnected += new Client.ClientDisconnectedHandler(TcpServer_ClientDisconnected);
        }

        private void TcpServer_ClientDisconnected(Client sender)
        {
            Program.Logger.Write("Close tcp connection");
        }
        #endregion

        private void TcpServer_ClientReceived(Client sender, byte[] data)
        {
            Parse(data);
        }

        private void UdpServer_Received(IPEndPoint sender, byte[] data)
        {
            Parse(data);
        }

        private void Parse(byte[] data)
        {
            if (data.Length < 2)
                return;
            OctetsStream stream = new OctetsStream(data);
            uint Opcode = stream.ReadCUInt();
            if (Enum.IsDefined(typeof(LogType), Opcode))
            {
                stream.ReadCUInt();
                try
                {
                    LogItem item = new LogItem(stream.ReadInt32(), stream.ReadString(OctetsString.GBK), stream.ReadString(OctetsString.GBK), stream.ReadString(OctetsString.GBK), Opcode);
                    bool IsChat = Utils.IsChat(item.Msg);
                    if (IsChat)
                        manager[Services.Chat]?.ProcessChat(item);
                    Services service = Utils.Cast(item.Servicename);
                    if (manager.ContainsKey(service))
                        ThreadPool.QueueUserWorkItem(manager[service].Process, item);
                    else
                        Program.Logger.Write($"Unknown service {item.Servicename}");
                    if (IsChat)
                    {
                        plugins.Where(x => x.ReqServices.HasFlag(Services.Chat) || x.ReqServices == Services.All).ToList().ForEach(x =>
                        {
                            try
                            {
                                x.Execute(item);
                            }
                            catch (Exception e)
                            {
                                Program.Logger.Write($"Plugin {x.Name} execute return error: {e.Message} | {e.StackTrace}");
                            }
                        });
                    }
                    else
                    {
                        plugins.Where(x => x.ReqServices.HasFlag(service) || x.ReqServices == Services.All).ToList().ForEach(x =>
                        {
                            try
                            {
                                x.Execute(item);
                            }
                            catch (Exception e)
                            {
                                Program.Logger.Write($"Plugin {x.Name} execute return error: {e.Message} | {e.StackTrace}");
                            }
                        });
                    }
                }
                catch (Exception e)
                {
                    Program.Logger.Write($"Error while reading message with opcode: {Opcode}, error: {e.Message} | {e.StackTrace}");
                }
            }
            else
            {
                Program.Logger.Write($"Error, unknown opcode {Opcode}");
            }
        }
    }
}
