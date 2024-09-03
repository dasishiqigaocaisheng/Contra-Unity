using UnityEngine;

namespace Modules.Utility.Singleton
{
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _Inst;

        public static T Inst
        {
            get
            {
                if (_Inst != null)
                    return _Inst;
                else
                {
                    //Debug.LogError("MonoSingleton: 单例还未被创建");
                    return null;
                }
            }
        }

        void Awake()
        {
            if (_Inst == null)
            {
                _Inst = (T)this;
                OnAwake();
            }
            else
                Destroy(gameObject);
        }

        protected virtual void OnAwake()
        {
        }
    }
}
