using Sirenix.OdinInspector;

namespace Modules.Utility.Flag
{
    public class Flag<T>
    {
        [ShowInInspector, LabelText("标志位")]
        //标志位
        private bool _Sign;
        //关联数据
        private T _Attached_Data;
        //关联数据默认值
        private T _Default_Data;

        [ShowInInspector, LabelText("关联数据")]
        public T Attached_Data { get => _Attached_Data; private set => _Attached_Data = value; }

        public Flag(T default_data = default)
        {
            _Default_Data = default_data;
        }

        /*置位*/
        public void Set(T data = default)
        {
            Attached_Data = data;
            _Sign = true;
        }

        /*复位*/
        public void Reset()
        {
            Attached_Data = _Default_Data;
            _Sign = false;
        }

        /*读取*/
        public bool Check(bool reset)
        {
            bool save = _Sign;
            if (reset)
                _Sign = false;

            return save;
        }

        /*读取*/
        public bool Check(bool reset, out T data)
        {
            data = Attached_Data;
            bool save = _Sign;
            if (reset)
                _Sign = false;

            return save;
        }

    }
}
