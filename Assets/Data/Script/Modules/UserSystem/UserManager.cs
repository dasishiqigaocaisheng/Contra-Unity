using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Modules;

namespace Modules.UserSystem
{
    public class UserManager : MonoBehaviour, IModule
    {
        private Dictionary<int, UserServiceSpace> _UserServiceSpaces = new Dictionary<int, UserServiceSpace>();

        private Dictionary<string, int> _Name2UID = new Dictionary<string, int>();

        public void Prepare() { }



    }
}
