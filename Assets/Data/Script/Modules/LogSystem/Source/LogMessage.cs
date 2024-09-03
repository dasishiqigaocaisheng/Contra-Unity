using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.LogSystem
{
    public class LogMessage
    {
        public readonly string Source;

        public readonly string Position;

        public readonly LogMessageType Type;

        public readonly string Message;

        public readonly Dictionary<string, object> Arguments;

        public LogMessage(LogMessageType type, string source, string position, string message, params (string, object)[] args)
        {
            Source = source;
            Position = position;
            Type = type;
            Message = message;

            if (args == null)
                return;

            Arguments = new Dictionary<string, object>();

            foreach (var arg in args)
                Arguments.Add(arg.Item1, arg.Item2);
        }
    }
}
