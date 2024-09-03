using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.FSM
{
    [Serializable]
    public class FSM
    {
        #region Members/Properties

        //下面是三个对常用转移条件的枚举
        //总是转移
        public readonly static FSM_TransferCondition ALWAYS = (x) => true;
        //永不转移
        public readonly static FSM_TransferCondition NEVER = (x) => false;
        //手动转移
        public readonly static FSM_TransferCondition MANUAL = (x) => false;

        //名称
        [SerializeField]
        private string _Name;
        public string Name { get => _Name; }

        //是否是根状态机
        [SerializeField]
        private bool _Is_Root;
        public bool Is_Root { get => _Is_Root; }

        //所属的FSM_Controller
        private FSM_Controller _Controller;
        public FSM_Controller Controller { get => _Controller; }

        //父状态（只有当此状态机是由某个状态变换得到的时，此变量才有意义）
        [SerializeReference]
        private FSM_State _Parent_State;
        public FSM_State Parent_State { get => _Parent_State; }

        //当前活跃状态
        [SerializeReference]
        public FSM_State Active_State;

        //状态的名称-实例字典
        [SerializeField]
        private Dictionary<string, FSM_State> _States = new Dictionary<string, FSM_State>();

        public FSM_State this[string str] { get => _States[str]; }

        #endregion

        #region Public Methods


        public FSM(string name, FSM_Controller controller)
        {
            //基本参数配置
            _Name = name;
            _Is_Root = true;
            _Controller = controller;
            _Parent_State = null;

            //创建__ENTER和__EXIT状态
            AddState("__ENTER");
            AddState("__EXIT",
                update: (x) =>
                {
                    if (_Parent_State == null)
                        return;

                    //在这里进行转移条件判断
                    foreach (FSM_TransferPath tp in x.Attached_FSM.Parent_State.TransferPaths)
                    {
                        if (tp.TransferCondition())
                        {
                            _Parent_State.GotoState(tp.Target_State.Name);
                            return;
                        }
                    }
                    //默认转移条件判定
                    if (_Parent_State.DefaultPath != null)
                    {
                        if (_Parent_State.DefaultPath.TransferCondition())
                            _Parent_State.GotoState(_Parent_State.DefaultPath.Target_State.Name);
                    }
                });

            //将活跃状态指向__ENTER
            Active_State = _States["__ENTER"];
        }

        public FSM(string name, FSM_State parent) : this(name, parent.Attached_FSM.Controller)
        {
            _Parent_State = parent;
        }

        /*
        *功能：
        *   为状态机添加状态
        *参数：
        *   1.name：状态名称
        *   2.[enter]：进入状态回调函数（全局）
        *   3.[fixedupdate]：FixedUpdate状态执行函数
        *   4.[update]：Update状态执行函数
        *   5.[lateupdate]：LateUpdate状态执行函数
        *   6.[gui]：OnGUI状态执行函数
        *   7.[exit]：离开状态回调函数（全局）
        *返回值：
        *   状态实例
        */
        public FSM_State AddState(string name, FSM_OnStateEnter enter = null, FSM_OnStateFixedUpdate fixedupdate = null, FSM_OnStateUpdate update = null,
                                                FSM_OnStateLateUpdate lateupdate = null, FSM_OnStateOnGUI gui = null, FSM_OnStateExit exit = null, FSM_EndOfFrame endoframe = null)
        {
            //创建新状态，并将其加入字典
            FSM_State state = new FSM_State(this, name);

            //添加事件
            if (enter != null) state.OnEnter += enter;
            if (fixedupdate != null) state.OnFixedUpdate += fixedupdate;
            if (update != null) state.OnUpdate += update;
            if (lateupdate != null) state.OnLateUpdate += lateupdate;
            if (gui != null) state.OnGUI += gui;
            if (exit != null) state.OnExit += exit;
            if (endoframe != null) state.OnFrameEnd += endoframe;

            _States.Add(name, state);

            return state;
        }

        /*
        *功能：
        *   设置启动路径
        *参数：
        *   1.state：启动状态
        *   2.cond：启动条件
        *   3.[enter]：进入（启动）状态回调函数
        */
        public void SetBeginPath(string state, FSM_TransferCondition cond, FSM_GetAttachedData get_attacheddata = null, FSM_OnTransfer transfer = null)
        {
            AddTransferPath("__ENTER", state, cond, get_attacheddata, transfer);
        }

        /*
        *功能：
        *   设置退出路径
        *参数：
        *   1.state：退出状态
        *   2.cond：退出条件
        *   3.[enter]：离开（退出）状态回调函数
        */
        //TODO:缺少转移回调
        public void SetExitPath(string state, FSM_TransferCondition cond, FSM_GetAttachedData get_attacheddata = null)
        {
            _States[state].AddTransferPath("__EXIT", cond, get_attacheddata);
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
        public void AddTransferPath(string s_state, string t_state, FSM_TransferCondition cond, FSM_GetAttachedData getdata = null, FSM_OnTransfer transfer = null)
        {
            _States[s_state].TransferPaths.Add(new FSM_TransferPath(_States[s_state], _States[t_state], cond, getdata, transfer));
        }

        /*
        *功能：
        *   手动进行一次状态转移
        *参数：
        *   1.name：转移的目的状态的名称
        */
        public void StateTransfer(string name)
        {
            Controller.StateTransfer(name);
        }

        /*
        *功能：
        *   手动进行一次状态转移
        *参数：
        *   1.name：转移的目的状态的名称
        *   2.attached_data：关联数据
        */
        public void StateTransfer(string name, object attached_data)
        {
            Controller.StateTransfer(name, attached_data);
        }

        /*public void Update()
        {
            Active_State.OnUpdateInvoke();
        }

        public void FixedUpdate()
        {
            Active_State.OnFixedUpdateInvoke();
        }

        public void LateUpdate()
        {
            Active_State.OnLateUpdateInvoke();
        }

        public void OnGUI()
        {
            Active_State.OnGUIInvoke();
        }

        public void FrameEnd()
        {
            Active_State.OnFrameEndInvoke();
        }*/

        #endregion


    }
}
