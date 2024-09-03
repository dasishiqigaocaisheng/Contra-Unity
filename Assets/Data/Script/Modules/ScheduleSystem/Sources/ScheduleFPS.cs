namespace Modules.ScheduleSystem
{
    /*
     *调度更新率枚举
     */
    public enum ScheduleFPS
    {
        FPS_1 = 1,      //1FPS
        FPS_2 = 2,      //2FPS
        FPS_4 = 4,      //4FPS
        FPS_8 = 8,      //8FPS
        FPS_16 = 16,    //16FPS
        FPS_32 = 32,    //32FPS
        FPS_64 = 64,    //64FPS

        //下面三种更新率是针对FixedUpdate的
        //且假设fixeddeltatime=0.02s
        FPS_12_5_FIXED = 12,    //12.5FPS
        FPS_25_FIXED = 25,
        FPS_50_FIXED = 50
    }
}
