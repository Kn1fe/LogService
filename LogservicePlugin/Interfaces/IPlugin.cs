namespace LogservicePlugin.Interfaces
{
    public interface IPlugin
    {
        string Name { get; }
        string Version { get; }
        string Author { get; }
        string BaseDirectory { get; set; }

        Services ReqServices { get; }

        void Initialize();
        void Execute(LogItem item);
    }
}
