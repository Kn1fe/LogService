using MadMilkman.Ini;

namespace LogservicePlugin.Interfaces
{
    public interface ISettings
    {
        IniFile Ini { get; set; }
    }
}
