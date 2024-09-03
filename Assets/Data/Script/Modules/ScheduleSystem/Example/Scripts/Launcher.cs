using UnityEngine;

namespace Modules.ScheduleSystem.Example
{
    public class Launcher : MonoBehaviour
    {
        private ScheduleTest _Cube0, _Cube1, _Cube2;

        private void Awake()
        {
            gameObject.AddComponent<ScheduleManager>();

            _Cube0 = GameObject.Find("Cube0").AddComponent<ScheduleTest>();
            _Cube1 = GameObject.Find("Cube1").AddComponent<ScheduleTest>();
            _Cube2 = GameObject.Find("Cube2").AddComponent<ScheduleTest>();

            ScheduleManager.Inst.TaskRegist(ScheduleTaskType.Update, _Cube0);
            ScheduleManager.Inst.TaskRegist(ScheduleTaskType.Update, _Cube1, fps0: ScheduleFPS.FPS_32);
            ScheduleManager.Inst.TaskRegist(ScheduleTaskType.Update, _Cube2, fps0: ScheduleFPS.FPS_16);
        }

        private void Start()
        {
            ScheduleManager.Inst.TaskRun(_Cube0);
            ScheduleManager.Inst.TaskRun(_Cube1);
            ScheduleManager.Inst.TaskRun(_Cube2);
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 50, 20), _Cube0.UpdateRate.ToString("f3"));
            GUI.Box(new Rect(10, 35, 50, 20), _Cube1.UpdateRate.ToString("f3"));
            GUI.Box(new Rect(10, 60, 50, 20), _Cube2.UpdateRate.ToString("f3"));
        }
    }
}
