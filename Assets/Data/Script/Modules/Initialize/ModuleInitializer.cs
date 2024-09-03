using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Modules
{
    public sealed class ModuleInitializer : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("模块初始化时的配置数据")]
        internal InitializeConfig Config;

        [SerializeField]
        [Tooltip("所有模块初始化结束回调")]
        private UnityEvent _OnPrepareFinished;

        private static bool _Initialized;

        private void Awake()
        {
            if (_Initialized)
                Destroy(this);

            if (Config == null)
            {
                Debug.LogError("ModuleInitializer: 模块初始化配置数据为空");
                return;
            }

            LogSystem.LogManager.Prepare();

            GameObject go = new GameObject("[Modules]");

            if (Config.DataIO)
                go.AddComponent<IOSystem.IOManager>().Prepare();

            if (Config.FSM)
                go.AddComponent<FSM.FSMManager>().Prepare();

            if (Config.ScheduleSystem)
                go.AddComponent<ScheduleSystem.ScheduleManager>().Prepare();

            if (Config.MessageSystem)
                go.AddComponent<MessageSystem.MessageManager>().Prepare();

            _Initialized = true;
        }

        private void Start()
        {
            _OnPrepareFinished.Invoke();
            Destroy(gameObject);
        }
    }
}
