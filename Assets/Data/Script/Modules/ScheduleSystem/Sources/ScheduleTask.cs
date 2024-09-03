namespace Modules.ScheduleSystem
{
    //任务委托
    internal delegate void ScheduleMethod(float delta_time);

    /*
     *调度任务
     *记录任务的更新率和任务实例
     */
    internal class ScheduleTask
    {
        public ISchedule Task;

        public ScheduleTaskType Type;

        public ScheduleFPS UpdateFPS;

        public ScheduleFPS FixedFPS;

        //运行中
        public bool Running = false;

        public ScheduleTask(ISchedule tsk, ScheduleTaskType type, ScheduleFPS upd_fps = ScheduleFPS.FPS_64, ScheduleFPS fupd_fps = ScheduleFPS.FPS_50_FIXED)
        {
            Task = tsk;
            Type = type;
            UpdateFPS = upd_fps;
            FixedFPS = fupd_fps;
        }
    }
}
