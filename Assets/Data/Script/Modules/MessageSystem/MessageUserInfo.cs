using System.Collections.Generic;
using UnityEngine;

namespace Modules.MessageSystem
{
    public struct MessageUserInfo
    {
        public int UID { get; internal set; }

        public string Name { get; internal set; }
    }

    internal struct MessageUser
    {
        public MessageUserInfo Info { get; set; }

        public MessageBox MsgBox { get; set; }
    }
}
