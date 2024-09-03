using System.Collections.Generic;
using UnityEngine;
using Modules.Utility;
using Modules.LogSystem;

namespace Modules.ScheduleSystem
{
    /*
     *调度时刻
     *同一个更新率（FPS）的任务（Task）放入同一个调度时刻中
     */
    internal class ScheduleTick
    {
        //该调度时刻的更新率
        public readonly ScheduleFPS FPS;

        //调度周期
        private readonly float _SchedulePeriod;

        //Update任务列表
        public List<ScheduleTask> Tasks { get; private set; } = new List<ScheduleTask>();

        //记录每一次更新后的时间
        private float _TimeReg;

        //搁置的任务注册/注销。在任务更新期间注册/注销的任务会先存入该队列，在帧的结尾统一操作
        //bool值代表该任务是注册还是注销（true：注册；false：注销）
        private Queue<(ScheduleTask, bool)> _TasksWaitForProcess = new Queue<(ScheduleTask, bool)>();

        public ScheduleTick(ScheduleFPS fps)
        {
            FPS = fps;
            _SchedulePeriod = fps switch
            {
                ScheduleFPS.FPS_1 => 1.0f / 1,
                ScheduleFPS.FPS_2 => 1.0f / 2,
                ScheduleFPS.FPS_4 => 1.0f / 4,
                ScheduleFPS.FPS_8 => 1.0f / 8,
                ScheduleFPS.FPS_16 => 1.0f / 16,
                ScheduleFPS.FPS_32 => 1.0f / 32,
                ScheduleFPS.FPS_64 => 1.0f / 64,
                ScheduleFPS.FPS_12_5_FIXED => 1.0f / 12.5f,
                ScheduleFPS.FPS_25_FIXED => 1.0f / 25,
                ScheduleFPS.FPS_50_FIXED => 1.0f / 50,
                _ => 1.0f
            };
        }

        public void AddTaskToWaitList(ScheduleTask tsk)
        {
            _TasksWaitForProcess.Enqueue((tsk, true));
        }

        public void RemoveTaskFromWaitList(ScheduleTask tsk)
        {
            tsk.Running = false;
            _TasksWaitForProcess.Enqueue((tsk, false));
        }

        /*
         *更新Update任务
         *参数：
         *  1.updt_time：这次更新帧数（包含补帧数）
         *  2.overed_time：相比基准更新时间（1/FPSbase）超时的时间，也就是To
         */
        public void Update()
        {
            foreach (ScheduleTask st in Tasks)
            {
                if (st.Running)
                    st.Task.Update(Time.time - _TimeReg);
            }
            _TimeReg = Time.time;
        }

        /*
         *更新LateUpdate任务
         */
        public void LateUpdate()
        {
            foreach (ScheduleTask st in Tasks)
            {
                if (st.Running)
                    st.Task.LateUpdate(Time.time - _TimeReg);
            }
            _TimeReg = Time.time;
        }

        public void FixedUpdate()
        {
            foreach (ScheduleTask st in Tasks)
            {
                if (st.Running)
                    st.Task.FixedUpdate(_SchedulePeriod);
            }
        }

        public void TaskListUpdate()
        {
            while (_TasksWaitForProcess.Count > 0)
            {
                (ScheduleTask ts, bool is_add) = _TasksWaitForProcess.Dequeue();
                //添加操作
                if (is_add)
                {
                    Tasks.Add(ts);
                    ts.Task.OnRegist();
                }
                //移除操作
                else
                    Tasks.Remove(ts);
            }
        }
    }
}
