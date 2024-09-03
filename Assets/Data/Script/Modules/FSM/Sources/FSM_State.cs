using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.FSM
{
    //进入状态全局回调函数
    //参数：path：转移路径
    public delegate void FSM_OnStateEnter(FSM_State state);
    //FixedUpdate状态执行函数
    public delegate void FSM_OnStateFixedUpdate(FSM_State state);
    //Update状态执行函数
    public delegate void FSM_OnStateUpdate(FSM_State state);
    //LateUpdate状态执行函数
    public delegate void FSM_OnStateLateUpdate(FSM_State state);
    //OnGUI状态执行函数
    public delegate void FSM_OnStateOnGUI(FSM_State state);
    //EndOfFrame状态执行函数
    public delegate void FSM_EndOfFrame(FSM_State state);
    //离开状态回调函数
    //参数：path：转移路径
    public delegate void FSM_OnStateExit(FSM_State state);

    [Serializable]
    public class FSM_State
    {
        #region Members/Properties

        //状态名称
        [SerializeField]
        private string _Name;
        public string Name { get => _Name; }

        //是否被转化为状态机
        [SerializeField]
        private bool _Is_SetAsFSM;
        public bool Is_SetAsFSM { get => _Is_SetAsFSM; }

        //属于哪一个状态机？
        [SerializeReference]
        private FSM _Attached_FSM;
        public FSM Attached_FSM { get => _Attached_FSM; }

        //子状态机（只有_Is_SetAsFSM==true，变量才有意义）
        [SerializeReference]
        private FSM _Child_FSM;
        public FSM Child_FSM { get => _Child_FSM; }

        //当其子状态机运行时，此状态仍然保持运行
        [SerializeField]
        private bool _Keep_Running;
        public bool Keep_Running { get => _Keep_Running; }

        //此状态是由哪个状态进入的？（只有此状态活跃时此变量才有意义）
        [SerializeReference]
        public FSM_State Enter_State;

        //状态转移路径列表
        private List<FSM_TransferPath> _TransferPaths = new List<FSM_TransferPath>();
        public List<FSM_TransferPath> TransferPaths { get => _TransferPaths; }

        //默认转移路径
        private FSM_TransferPath _DefaultPath;
        public FSM_TransferPath DefaultPath { get => _DefaultPath; }

        //关联数据
        [SerializeField]
        public object Attached_Data;

        //事件
        public event FSM_OnStateEnter OnEnter;
        public event FSM_OnStateFixedUpdate OnFixedUpdate;
        public event FSM_OnStateUpdate OnUpdate;
        public event FSM_OnStateLateUpdate OnLateUpdate;
        public event FSM_OnStateOnGUI OnGUI;
        public event FSM_EndOfFrame OnFrameEnd;
        public event FSM_OnStateExit OnExit;

        #endregion

        #region Public Methods

        /*
        *功能：
        *   构造函数
        *参数：
        *   1.fsm：此状态的状态机
        *   2.name：状态名称
        *   3.[enter]：进入状态回调函数
        *   4.[fixedupdate]：FixedUpdate状态执行函数
        *   4.[update]：Update状态执行函数
        *   5.[lateupdate]：LateUpdate状态执行函数
        *   6.[gui]：OnGUI状态执行函数
        *   7.[exit]：离开状态回调函数
        */
        public FSM_State(FSM fsm, string name)
        {
            _Name = name;
            _Attached_FSM = fsm;
        }

        /*
        *功能：
        *   将此状态转换为状态机
        *参数：
        *   1.name：状态机名称
        *返回值：
        *   转换的状态机实例
        */
        public FSM SetAsFSM(string name, bool keep_running)
        {
            _Child_FSM = new FSM(name, this);
            _Is_SetAsFSM = true;
            _Keep_Running = keep_running;

            return Child_FSM;
        }

        public T GetAttachedData<T>()
        {
            return (T)Attached_Data;
        }

        /*
        *功能：
        *   为状态机添加转移路径
        *参数：
        *   1.s_state：源状态的名称
        *   2.t_state：目标状态名称
        *   3.cond：转移条件
        *   4.[exit]：离开（源）状态回调函数
        *   5.[enter]：进入（目标）状态回调函数
        */
        public void AddTransferPath(string t_state, FSM_TransferCondition cond, FSM_GetAttachedData get_attacheddata = null)
        {
            _TransferPaths.Add(new FSM_TransferPath(this, _Attached_FSM[t_state], cond, get_attacheddata));
        }

        /*
        *功能：
        *   设置状态的默认转移路径
        *参数：
        *   1.s_state：源状态的名称
        *   2.t_state：目标状态名称
        *   3.cond：转移条件
        *   4.[exit]：离开（源）状态回调函数
        *   5.[enter]：进入（目标）状态回调函数
        */
        public void SetDefaultPath(string t_state, FSM_TransferCondition cond, FSM_GetAttachedData get_attacheddata = null)
        {
            _DefaultPath = new FSM_TransferPath(this, _Attached_FSM[t_state], cond, get_attacheddata);
        }

        public void GotoState(string name)
        {
            _Attached_FSM.StateTransfer(name);
        }

        public void GotoState(string name, object attached_data)
        {
            _Attached_FSM.StateTransfer(name, attached_data);
        }

        public void Exit()
        {
            _Attached_FSM.StateTransfer("__EXIT");
        }

        public void OnEnterInvoke()
        {
            OnEnter?.Invoke(this);
        }

        public void OnFixedUpdateInvoke()
        {
            OnFixedUpdate?.Invoke(this);
        }

        public void OnUpdateInvoke()
        {
            OnUpdate?.Invoke(this);
        }

        public void OnLateUpdateInvoke()
        {
            OnLateUpdate?.Invoke(this);
        }

        public void OnGUIInvoke()
        {
            OnGUI?.Invoke(this);
        }

        public void OnFrameEndInvoke()
        {
            OnFrameEnd?.Invoke(this);
        }

        public void OnExitInvoke()
        {
            OnExit?.Invoke(this);
        }



        #endregion

    }
}
