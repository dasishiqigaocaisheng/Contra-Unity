namespace Modules.Utility.Singleton
{
    public class Singleton<T> where T : Singleton<T>, new()
    {
        public Singleton() { }
        public static readonly T Inst = (T)new T();
    }
}
