using UnityEngine;

namespace Modules.ScheduleSystem.Example
{
    public class ScheduleTest : MonoBehaviour, ISchedule
    {
        private int _State = 0;

        private float _Timer;

        private int _Counter;

        public float UpdateRate;

        void ISchedule.OnRegist()
        {
            Debug.Log("Task has registed.");
        }

        void ISchedule.Update(float delta_time)
        {
            _Counter++;
            if (_Counter == 50)
            {
                _Timer = Time.time - _Timer;
                UpdateRate = 50 / _Timer;
                _Timer = Time.time;
                _Counter = 0;
            }

            switch (_State)
            {
                //向z+运动（Tgt0）
                case 0:
                    {
                        transform.Translate(new Vector3(0, 0, 1) / 8);
                        if (10 - transform.position.z < 0.1f)
                            _State = 1;
                        break;
                    }
                //向z-运动
                case 1:
                    {
                        transform.Translate(new Vector3(0, 0, -1) / 8);
                        if (transform.position.z < 0.1f)
                            _State = 0;
                        break;
                    }
                default:
                    break;
            }
        }

        void ISchedule.LateUpdate(float dt)
        {

        }

        void ISchedule.FixedUpdate(float dt)
        {

        }
    }
}
