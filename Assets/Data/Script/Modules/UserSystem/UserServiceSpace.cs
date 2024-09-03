using System;
using System.Collections.Generic;
using UnityEngine;
using Modules.LogSystem;

namespace Modules.UserSystem
{
    internal class UserServiceSpace
    {
        private Dictionary<Type, IServiceSpace> _ServiceSpaces = new Dictionary<Type, IServiceSpace>();

        public IServiceSpace GetService<T>() where T : IModule
        {
            if (_ServiceSpaces.TryGetValue(typeof(T), out IServiceSpace ss))
                return ss;

            return null;
        }

        public IServiceSpace AddServiceSpace<T1, T2>() where T1 : IModule where T2 : IServiceSpace, new()
        {
            LogManager.Assert(!_ServiceSpaces.ContainsKey(typeof(T1)), nameof(UserServiceSpace), $"模块{typeof(T1)}的服务空间已经存在");

            T2 ss = new T2();
            _ServiceSpaces.Add(typeof(T1), ss);
            ss.Init();

            return ss;
        }
    }
}
