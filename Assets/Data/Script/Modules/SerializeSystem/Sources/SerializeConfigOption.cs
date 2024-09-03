using System;

namespace Modules.SerializeSystem
{
    [Flags]
    public enum SerializeConfigOption
    {
        Default = 0,
        DelayDefine = 1,
        ElementsDelayDefine = 1 << 1,
        KeyDelayDefine = 1 << 2,
        ValueDelayDefine = 1 << 3,
        Extern = 1 << 4
    }
}
