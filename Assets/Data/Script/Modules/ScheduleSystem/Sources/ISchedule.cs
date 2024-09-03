namespace Modules.ScheduleSystem
{
    /*
     * 调度接口
     * 只有实现该接口的类，才能被ScheduleSystem调度
     */
    public interface ISchedule
    {
        void OnRegist();
        void Update(float dt);
        void FixedUpdate(float dt);
        void LateUpdate(float dt);
    }
}
