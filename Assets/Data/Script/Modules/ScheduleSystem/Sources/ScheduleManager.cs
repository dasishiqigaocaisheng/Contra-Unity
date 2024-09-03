using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Modules.LogSystem;
using Modules.Utility.Singleton;

namespace Modules.ScheduleSystem
{
    internal class IdleTask : ISchedule
    {
        public void OnRegist() { }
        public void Update(float dt) { }
        public void FixedUpdate(float dt) { }
        public void LateUpdate(float dt) { }
    }

    /*
     * 调度相位（*代表执行，-代表空闲）
     * |******************************************************************************************************************|（64Hz）
     * |*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-|（32Hz）
     * |-*---*---*---*---*---*---*---*---*---*---*---*---*---*---*---*---*---*---*---*---*---*---*---*---*---*---*---*---*|（16Hz）
     * |---*-------*-------*-------*-------*-------*-------*-------*-------*-------*-------*-------*-------*-------*------|（8Hz）
     * |-------*---------------*---------------*---------------*---------------*---------------*---------------*----------|（4Hz）
     * |---------------*-------------------------------*-------------------------------*-------------------------------*--|（2Hz）
     * |-------------------------------*---------------------------------------------------------------*------------------|（1Hz）
     * 
     */
    public class ScheduleManager : MonoSingleton<ScheduleManager>, IModule
    {
        //任务注册表
        private Dictionary<ISchedule, ScheduleTask> _RegistedTask = new Dictionary<ISchedule, ScheduleTask>();

        //Update任务队列
        private ScheduleTick[] _UpdateQueue;

        //FixedUpdate任务队列
        private ScheduleTick[] _FixedUpdateQueue;

        private int[][] _SchedulePhase;

        private float _TimeReg;

        private int _LastUpdatePhase;

        private bool _UpdateThisFrame;

        private int _LastFixedUpdatePhase;



        public void Prepare()
        {
            LogManager.Info("ScheduleSystem.ScheduleManager", "ScheduleSystem已经初始化");
        }

        private ScheduleTick _GetScheduleUpdateTick(ScheduleFPS fps)
        {
            return fps switch
            {
                ScheduleFPS.FPS_1 => _UpdateQueue[6],
                ScheduleFPS.FPS_2 => _UpdateQueue[5],
                ScheduleFPS.FPS_4 => _UpdateQueue[4],
                ScheduleFPS.FPS_8 => _UpdateQueue[3],
                ScheduleFPS.FPS_16 => _UpdateQueue[2],
                ScheduleFPS.FPS_32 => _UpdateQueue[1],
                ScheduleFPS.FPS_64 => _UpdateQueue[0],
                ScheduleFPS.FPS_50_FIXED => _FixedUpdateQueue[0],
                ScheduleFPS.FPS_25_FIXED => _FixedUpdateQueue[1],
                ScheduleFPS.FPS_12_5_FIXED => _FixedUpdateQueue[2],
                _ => null
            };
        }

        /*
         *注册任务
         */
        public void TaskRegist(ScheduleTaskType type, ISchedule tsk, int pri = 0, ScheduleFPS fps0 = ScheduleFPS.FPS_64, ScheduleFPS fps1 = ScheduleFPS.FPS_50_FIXED)
        {
            if (tsk is null)
            {
                LogManager.Error("ScheduleSystem", "参数（tsk）为null");
                return;
            }

            //首先检查任务是否已注册
            if (_RegistedTask.ContainsKey(tsk))
            {
                LogManager.Error("ScheduleSystem", "任务（tsk）重复注册");
                return;
            }

            ScheduleTask st = new ScheduleTask(tsk, type, fps0, fps1);
            //放入搁置区
            if (st.Type.HasFlag(ScheduleTaskType.Update))
                _GetScheduleUpdateTick(st.UpdateFPS).AddTaskToWaitList(st);
            if (st.Type.HasFlag(ScheduleTaskType.FixedUpdate))
                _GetScheduleUpdateTick(st.FixedFPS).AddTaskToWaitList(st);
            //存入任务列表
            _RegistedTask.Add(st.Task, st);
        }

        /*
         *注销任务
         */
        public void TaskUnregist(ISchedule tsk)
        {
            if (tsk is null)
            {
                LogManager.Error("ScheduleSystem", "参数（tsk）为null");
                return;
            }

            //检查任务是否已经注册
            if (!_RegistedTask.TryGetValue(tsk, out ScheduleTask st))
            {
                LogManager.Error("ScheduleSystem", "要注销的任务（tsk）还没有注册");
                return;
            }

            //存入搁置区
            if (st.Type.HasFlag(ScheduleTaskType.Update))
                _GetScheduleUpdateTick(st.UpdateFPS).RemoveTaskFromWaitList(st);
            if (st.Type.HasFlag(ScheduleTaskType.FixedUpdate))
                _GetScheduleUpdateTick(st.FixedFPS).RemoveTaskFromWaitList(st);
            //从任务列表移除，此时该任务还存在于各Tick的列表中
            _RegistedTask.Remove(tsk);
        }

        /*
         *运行任务
         */
        public void TaskRun(ISchedule tsk)
        {
            if (tsk is null)
            {
                LogManager.Error("ScheduleSystem", "参数（tsk）为null");
                return;
            }

            if (!_RegistedTask.TryGetValue(tsk, out ScheduleTask st))
            {
                LogManager.Error("ScheduleSystem", "要运行的任务（tsk）还没有注册");
                return;
            }

            st.Running = true;
        }

        /*
         *停止任务
         */
        public void TaskStop(ISchedule tsk)
        {
            if (tsk is null)
            {
                LogManager.Error("ScheduleSystem", "参数（tsk）为null");
                return;
            }

            if (!_RegistedTask.TryGetValue(tsk, out ScheduleTask st))
            {
                LogManager.Error("ScheduleSystem", "要停止的任务（tsk）还没有注册");
                return;
            }

            st.Running = false;
        }

        protected override void OnAwake()
        {
            DontDestroyOnLoad(gameObject);

            //初始化调度队列
            _UpdateQueue = new ScheduleTick[]
            {
                    new ScheduleTick(ScheduleFPS.FPS_64),
                    new ScheduleTick(ScheduleFPS.FPS_32),
                    new ScheduleTick(ScheduleFPS.FPS_16),
                    new ScheduleTick(ScheduleFPS.FPS_8),
                    new ScheduleTick(ScheduleFPS.FPS_4),
                    new ScheduleTick(ScheduleFPS.FPS_2),
                    new ScheduleTick(ScheduleFPS.FPS_1),
                    null
            };

            _FixedUpdateQueue = new ScheduleTick[]
            {
                    new ScheduleTick(ScheduleFPS.FPS_50_FIXED),
                    new ScheduleTick(ScheduleFPS.FPS_25_FIXED),
                    new ScheduleTick(ScheduleFPS.FPS_12_5_FIXED),
                    null
            };

            //初始化调度相位
            _SchedulePhase = new int[][]
            {
                    new int[] {0,1},
                    new int[] {0,2},
                    new int[] {0,1},
                    new int[] {0,3},
                    new int[] {0,1},
                    new int[] {0,2},
                    new int[] {0,1},
                    new int[] {0,4},
                    new int[] {0,1},
                    new int[] {0,2},
                    new int[] {0,1},
                    new int[] {0,3},
                    new int[] {0,1},
                    new int[] {0,2},
                    new int[] {0,1},
                    new int[] {0,5},
                    new int[] {0,1},
                    new int[] {0,2},
                    new int[] {0,1},
                    new int[] {0,3},
                    new int[] {0,1},
                    new int[] {0,2},
                    new int[] {0,1},
                    new int[] {0,4},
                    new int[] {0,1},
                    new int[] {0,2},
                    new int[] {0,1},
                    new int[] {0,3},
                    new int[] {0,1},
                    new int[] {0,2},
                    new int[] {0,1},
                    new int[] {0,6},
                    new int[] {0,1},
                    new int[] {0,2},
                    new int[] {0,1},
                    new int[] {0,3},
                    new int[] {0,1},
                    new int[] {0,2},
                    new int[] {0,1},
                    new int[] {0,4},
                    new int[] {0,1},
                    new int[] {0,2},
                    new int[] {0,1},
                    new int[] {0,3},
                    new int[] {0,1},
                    new int[] {0,2},
                    new int[] {0,1},
                    new int[] {0,5},
                    new int[] {0,1},
                    new int[] {0,2},
                    new int[] {0,1},
                    new int[] {0,3},
                    new int[] {0,1},
                    new int[] {0,2},
                    new int[] {0,1},
                    new int[] {0,4},
                    new int[] {0,1},
                    new int[] {0,2},
                    new int[] {0,1},
                    new int[] {0,3},
                    new int[] {0,1},
                    new int[] {0,2},
                    new int[] {0,1},
                    new int[] {0,7}
            };

            TaskRegist(ScheduleTaskType.Update | ScheduleTaskType.FixedUpdate, new IdleTask(), 0, ScheduleFPS.FPS_1, ScheduleFPS.FPS_12_5_FIXED);
            TaskRegist(ScheduleTaskType.Update | ScheduleTaskType.FixedUpdate, new IdleTask(), 0, ScheduleFPS.FPS_2, ScheduleFPS.FPS_25_FIXED);
            TaskRegist(ScheduleTaskType.Update | ScheduleTaskType.FixedUpdate, new IdleTask(), 0, ScheduleFPS.FPS_4, ScheduleFPS.FPS_50_FIXED);
            TaskRegist(ScheduleTaskType.Update, new IdleTask(), 0, ScheduleFPS.FPS_8);
            TaskRegist(ScheduleTaskType.Update, new IdleTask(), 0, ScheduleFPS.FPS_16);
            TaskRegist(ScheduleTaskType.Update, new IdleTask(), 0, ScheduleFPS.FPS_32);
            TaskRegist(ScheduleTaskType.Update, new IdleTask(), 0, ScheduleFPS.FPS_64);
            foreach (var item in _RegistedTask)
                item.Value.Running = true;
        }

        private void Start()
        {
            StartCoroutine(_OnFrameEnd());
        }

        private void FixedUpdate()
        {
            _LastFixedUpdatePhase = (_LastFixedUpdatePhase + 1) % 4;
            _FixedUpdateQueue[_SchedulePhase[_LastFixedUpdatePhase][0]].FixedUpdate();
            _FixedUpdateQueue[_SchedulePhase[_LastFixedUpdatePhase][1]]?.FixedUpdate();
        }

        private void Update()
        {
            int upd_num = (int)((_TimeReg + Time.deltaTime) * 64.0f);
            if (upd_num == 1)
            {
                _UpdateQueue[_SchedulePhase[(_LastUpdatePhase + 1) % 64][0]].Update();
                _UpdateQueue[_SchedulePhase[(_LastUpdatePhase + 1) % 64][1]]?.Update();
                _LastUpdatePhase++;
                _TimeReg = Time.deltaTime + _TimeReg - 1.0f / 64;
                _UpdateThisFrame = true;
            }
            else if (upd_num == 0)
                _TimeReg += Time.deltaTime;
            else
            {
                _UpdateQueue[_SchedulePhase[(_LastUpdatePhase + 1) % 64][0]].Update();
                _UpdateQueue[_SchedulePhase[(_LastUpdatePhase + 1) % 64][1]]?.Update();
                _LastUpdatePhase++;
                _TimeReg = 0;
                _UpdateThisFrame = true;
            }

            _LastUpdatePhase %= 64;
        }

        private void LateUpdate()
        {
            if (_UpdateThisFrame)
            {
                int temp;
                if (_LastUpdatePhase == 0)
                    temp = 63;
                else
                    temp = _LastUpdatePhase - 1;

                _UpdateQueue[_SchedulePhase[temp][0]].LateUpdate();
                _UpdateQueue[_SchedulePhase[temp][1]]?.LateUpdate();
                _UpdateThisFrame = false;
            }
        }

        private IEnumerator _OnFrameEnd()
        {
            while (true)
            {
                foreach (ScheduleTick tick in _UpdateQueue)
                    tick?.TaskListUpdate();
                foreach (ScheduleTick tick in _FixedUpdateQueue)
                    tick?.TaskListUpdate();

                yield return new WaitForEndOfFrame();
            }
        }
    }
}
