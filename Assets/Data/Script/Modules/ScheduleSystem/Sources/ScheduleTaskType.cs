using System;

namespace Modules.ScheduleSystem
{
    [Flags]
    public enum ScheduleTaskType
    {
        Update = 1,
        FixedUpdate = 1 << 2
    }
}
