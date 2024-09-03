using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.MessageSystem
{
    public delegate bool FilterMatchFunc(in GameMessage gm);

    public delegate void MessageReceivedCallback(in GameMessage gm);

    internal class MessageFilter
    {
        public string Name { get; set; }

        public bool UpdateThisFrame { get; set; }

        public FilterMatchFunc MatchFunc { get; set; }

        public MessageReceivedCallback Callback { get; set; }

        public Queue<GameMessage> MsgQueue { get; } = new Queue<GameMessage>();
    }
}
