using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.UserSystem
{
    internal record UserInfo
    {
        public string UserName { get; set; }

        public int UID { get; set; }
    }
}
