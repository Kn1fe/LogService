using System;

namespace LogservicePlugin
{
    [Flags]
    public enum Services
    {
        Nan = 0,
        Gacd = 1,
        Gamed = 2,
        Gamedbd = 3,
        Gdeliveryd = 4,
        Uniquenamed = 5,
        Gfaction = 6,
        Glinkd = 7,
        Chat = 8,
        All = 9
    }
}
