using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Modules
{
    [CreateAssetMenu(fileName = "InitializeConfig", menuName = "Modules/初始化配置", order = 0)]
    internal class InitializeConfig : ScriptableObject
    {
        [Header("包含的功能模块")]
        public bool DataIO;

        public bool FSM;

        public bool ScheduleSystem;

        public bool MessageSystem;

        public bool UIFramework;
    }
}
