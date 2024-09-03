using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.LogSystem
{
    public class GameException : Exception
    {
        public readonly DateTime Time;

        public GameException(string message, params (string, object)[] data) : base(message)
        {
            Time = DateTime.Now;

            foreach (var item in data)
                Data.Add(item.Item1, item.Item2);
        }

        public static void ArgNullThrow(string arg)
        {
            throw new GameException($"参数（{arg}）为Null");
        }

        public static void ArgInvalidThrow(string arg, object value = null)
        {
            throw new GameException($"参数（{arg}）的值（{value?.ToString()}）不合法");
        }

        public override string ToString()
        {
            return $"{Message}\r\n";
        }
    }
}
