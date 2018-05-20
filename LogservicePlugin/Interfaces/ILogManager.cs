namespace LogservicePlugin.Interfaces
{
    public interface ILogManager
    {
        bool Decode { get; set; }
        void Process(object param);
        void ProcessChat(object param);
    }
}
