namespace Modules.FSM
{
    public delegate object FSM_GetAttachedData(FSM_TransferPath tp);
    public delegate bool FSM_TransferCondition(FSM_TransferPath tp);
    public delegate void FSM_OnTransfer(FSM_TransferPath tp);

    public class FSM_TransferPath
    {
        //源状态
        private FSM_State _Source_State;
        public FSM_State Source_State { get => _Source_State; }

        //目标状态
        private FSM_State _Target_State;
        public FSM_State Target_State { get => _Target_State; }

        //是否允许自动获取Attached_Data
        public bool Allow_AutoGet_AttachedData = true;

        //手动设定关联数据
        public object Attached_Data;

        //自动获取关联数据的委托
        private FSM_GetAttachedData _Get_AttachedData;

        //转移条件
        private FSM_TransferCondition _Condition;

        //路径转移回调函数
        public event FSM_OnTransfer OnTransfer;


        /*
        *构造函数
        *参数：
        *   1.s_state：源状态
        *   2.t_state：目标状态
        *   3.cond：转移条件
        *   4.transfer：状态转移事件回调函数
        */
        public FSM_TransferPath(FSM_State s_state, FSM_State t_state, FSM_TransferCondition cond, FSM_GetAttachedData get_attacheddata = null, FSM_OnTransfer transfer = null)
        {
            _Source_State = s_state;
            _Target_State = t_state;
            _Condition = cond;
            _Get_AttachedData = get_attacheddata;

            if (transfer != null) OnTransfer += transfer;
        }

        public bool TransferCondition()
        {
            return _Condition(this);
        }

        public object GetAttachedData()
        {
            if (_Get_AttachedData != null)
                return _Get_AttachedData(this);
            else
                return null;
        }

        public void OnTransferInvoke()
        {
            OnTransfer?.Invoke(this);
        }
    }
}
