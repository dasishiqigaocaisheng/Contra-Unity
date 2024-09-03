using System;

namespace Modules.SerializeSystem
{
    [Flags]
    public enum SerializeFormatOption
    {
        Default = 0,
        Compact = 1,
        Auto = 1 << 1
    }
}
