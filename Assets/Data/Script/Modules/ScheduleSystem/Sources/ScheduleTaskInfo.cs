namespace Modules.ScheduleSystem
{
    /*
     *任务注册信息
     *记录在任务注册表中
     */
    public struct ScheduleTaskInfo
    {
        public ISchedule Task;

        public ScheduleTaskType Type;

        public ScheduleFPS UpdateFPS;

        public ScheduleFPS FixedFPS;

        public int Priority;

        public ScheduleTaskInfo(ISchedule tsk, ScheduleTaskType type, ScheduleFPS fps0, ScheduleFPS fps1, int pri)
        {
            Task = tsk;
            Type = type;
            UpdateFPS = fps0;
            FixedFPS = fps1;
            Priority = pri;
        }
    }
}
