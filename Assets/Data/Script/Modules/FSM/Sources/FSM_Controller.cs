using System;
using System.Collections;
using UnityEngine;

namespace Modules.FSM
{
    internal class FSM_WaitBlockCancel : CustomYieldInstruction
    {
        public FSM_Controller Master;
        public override bool keepWaiting => Master.Is_Blocking;
    }

    //TODO:必须执行LateUpdate才能进行状态转移，这是多余的
    public class FSM_Controller
    {
        #region Members/Properties

        //当前活跃的FSM
        [SerializeReference]
        FSM _Active_FSM;
        public FSM Active_FSM { get => _Active_FSM; }

        //根状态机
        [SerializeReference]
        FSM _Root_FSM;
        public FSM Root_FSM { get => _Root_FSM; }

        //是否在运行
        [SerializeField]
        private bool _Is_Running;
        public bool Is_Running { get => _Is_Running; }

        //是否被阻塞
        [SerializeField]
        private bool _Is_Blocking;
        public bool Is_Blocking { get => _Is_Blocking; }

        //是否手动执行
        public bool ManuallyExecute { get; set; }

        //是否有状态转移挂起
        public bool Is_Transfer_Pending { get; private set; }

        //是否正在进行状态转移
        private bool _Is_Transfering;

        //被阻塞的状态转移路径
        private FSM_TransferPath _Blocking_Path;

        //等待阻塞结束的协程
        private FSM_WaitBlockCancel _WaitBlock_Coroutine = new FSM_WaitBlockCancel();

        #endregion


        public FSM_Controller()
        {
            _Root_FSM = new FSM("Root", this);
            _Active_FSM = _Root_FSM;
            _WaitBlock_Coroutine.Master = this;

            FSMManager.Inst.RegistFSMC(this);
        }

        public void FixedUpdate(bool transfer_check = false)
        {
            if (_Is_Transfering || !_Is_Running || Is_Transfer_Pending)
                return;

            if (transfer_check)
            {
                //在这里进行转移条件判断
                foreach (FSM_TransferPath tp in _Active_FSM.Active_State.TransferPaths)
                {
                    if (tp.TransferCondition())
                    {
                        Is_Transfer_Pending = true;
                        _Blocking_Path = tp;
                        return;
                    }
                }
                //默认转移条件判定
                if (_Active_FSM.Active_State.DefaultPath != null)
                {
                    if (_Active_FSM.Active_State.DefaultPath.TransferCondition())
                    {
                        Is_Transfer_Pending = true;
                        _Blocking_Path = _Active_FSM.Active_State.DefaultPath;
                    }
                }
            }

            FSM_State state = _Root_FSM.Active_State;
            while (state.Is_SetAsFSM)
            {
                if (state.Keep_Running)
                    state.OnFixedUpdateInvoke();
                state = state.Child_FSM.Active_State;
            }
            state.OnFixedUpdateInvoke();
        }

        public void Update()
        {
            if (_Is_Transfering || !_Is_Running || Is_Transfer_Pending)
                return;

            //在这里进行转移条件判断
            foreach (FSM_TransferPath tp in _Active_FSM.Active_State.TransferPaths)
            {
                if (tp.TransferCondition())
                {
                    Is_Transfer_Pending = true;
                    _Blocking_Path = tp;
                    return;
                }
            }
            //默认转移条件判定
            if (_Active_FSM.Active_State.DefaultPath != null)
            {
                if (_Active_FSM.Active_State.DefaultPath.TransferCondition())
                {
                    Is_Transfer_Pending = true;
                    _Blocking_Path = _Active_FSM.Active_State.DefaultPath;
                }
            }

            //如果有转移挂起
            //if (Is_Transfer_Pending)
            //    return;

            FSM_State state = _Root_FSM.Active_State;
            while (state.Is_SetAsFSM)
            {
                if (state.Keep_Running)
                    state.OnUpdateInvoke();
                state = state.Child_FSM.Active_State;
            }
            state.OnUpdateInvoke();
        }

        public void LateUpdate()
        {
            if (_Is_Transfering || !_Is_Running)
                return;

            //被挂起的状态转移在这里进行
            if (Is_Transfer_Pending)
            {
                FSMManager.Inst.StartCoroutine(_State_Transfer_Process(_Blocking_Path));
                Is_Transfer_Pending = false;
                return;
            }

            if (_Is_Transfering)
                return;

            FSM_State state = _Root_FSM.Active_State;
            while (state.Is_SetAsFSM)
            {
                if (state.Keep_Running)
                    state.OnLateUpdateInvoke();
                state = state.Child_FSM.Active_State;
            }
            state.OnLateUpdateInvoke();
        }

        public void OnGUI()
        {
            if (_Is_Transfering || !_Is_Running || Is_Transfer_Pending)
                return;

            FSM_State state = _Root_FSM.Active_State;
            while (state.Is_SetAsFSM)
            {
                if (state.Keep_Running)
                    state.OnGUIInvoke();
                state = state.Child_FSM.Active_State;
            }
            state.OnGUIInvoke();
        }

        public void FrameEnd()
        {
            if (_Is_Transfering || !_Is_Running || Is_Transfer_Pending)
                return;

            FSM_State state = _Root_FSM.Active_State;
            while (state.Is_SetAsFSM)
            {
                if (state.Keep_Running)
                    state.OnFrameEndInvoke();
                state = state.Child_FSM.Active_State;
            }
            state.OnFrameEndInvoke();
        }


        #region Public Methods

        public void Run()
        {
            _Is_Running = true;
        }

        public void Stop()
        {
            _Is_Running = false;
            _Active_FSM = _Root_FSM;
            _Root_FSM.Active_State = _Root_FSM["__ENTER"];
        }

        public void Pause()
        {
            _Is_Running = false;
        }

        public void Resume()
        {
            Run();
        }

        public void Block()
        {
            _Is_Blocking = true;
        }

        public void CancelBlock()
        {
            _Is_Blocking = false;
        }

        /*
        *功能：
        *   手动进行一次状态转移
        *参数：
        *   1.name：转移的目的状态的名称
        */
        public void StateTransfer(string name)
        {
            //TODO:需要完善：在已经进入转移状态下，不能进行转移
            FSM_State state = _Active_FSM[name];
            foreach (FSM_TransferPath tp in _Active_FSM.Active_State.TransferPaths)
            {
                if (tp.Target_State == state)
                {
                    tp.Allow_AutoGet_AttachedData = true;
                    Is_Transfer_Pending = true;
                    _Blocking_Path = tp;
                    return;
                }
            }
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
            FSM_State state = _Active_FSM[name];
            foreach (FSM_TransferPath tp in _Active_FSM.Active_State.TransferPaths)
            {
                if (tp.Target_State == state)
                {
                    tp.Attached_Data = attached_data;
                    tp.Allow_AutoGet_AttachedData = false;
                    Is_Transfer_Pending = true;
                    _Blocking_Path = tp;
                    return;
                }
            }
        }

        public void StateExit()
        {
            StateTransfer("__EXIT");
        }

        #endregion

        #region Private Methods

        /*
        *功能：
        *   执行状态转移进程
        *参数：
        *   1.path：转移路径
        */
        IEnumerator _State_Transfer_Process(FSM_TransferPath tp)
        {
            //进入转移模式
            _Is_Transfering = true;

            yield return new WaitForEndOfFrame();

            //出状态事件
            tp.Source_State.OnExitInvoke();
            if (Is_Blocking)
                yield return _WaitBlock_Coroutine;

            //转移事件
            tp.OnTransferInvoke();
            if (Is_Blocking)
                yield return _WaitBlock_Coroutine;

            //如果允许自动获取数据
            if (tp.Allow_AutoGet_AttachedData)
                tp.Attached_Data = tp.GetAttachedData();

            //状态转移
            _Active_FSM.Active_State = tp.Target_State;
            _Active_FSM.Active_State.Enter_State = tp.Source_State;
            tp.Target_State.Attached_Data = tp.Attached_Data;

            //入状态
            tp.Target_State.OnEnterInvoke();
            if (Is_Blocking)
                yield return _WaitBlock_Coroutine;

            //跨状态机检测
            if (_Active_FSM.Active_State.Is_SetAsFSM)
            {
                _Active_FSM = _Active_FSM.Active_State.Child_FSM;
                _Active_FSM.Active_State = _Active_FSM["__ENTER"];
            }
            else if (_Active_FSM.Active_State.Name == "__EXIT")
            {
                if (_Active_FSM.Parent_State != null)
                {
                    _Active_FSM.Active_State = _Active_FSM["__EXIT"];
                    _Active_FSM = _Active_FSM.Parent_State.Attached_FSM;
                }
            }

            //离开转移模式
            _Is_Transfering = false;
        }

        #endregion
    }
}
