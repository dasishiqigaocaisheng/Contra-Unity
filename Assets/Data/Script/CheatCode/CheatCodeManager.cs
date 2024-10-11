using System;
using System.Collections.Generic;

namespace Contra.CheatCode
{
    public enum CheatCodeKey
    {
        Up,
        Down,
        Left,
        Right,
        A,
        B
    }

    public static class CheatCodeManager
    {
        private static List<CheatCode> _CheatCodes = new List<CheatCode>();

        private static List<Action<Player>> _Callbacks = new List<Action<Player>>();

        public static void Push(Player ch, CheatCodeKey key)
        {
            for (int i = 0; i < _CheatCodes.Count; i++)
            {
                if (_CheatCodes[i].CheckOnce(ch, key))
                {
                    _Callbacks[i](ch);
                    _CheatCodes[i].Reset(ch);
                }
            }
        }

        public static void AddCheatCode(string name, CheatCodeKey[] keys, Action<Player> cb)
        {
            CheatCode cc = new CheatCode();
            cc.Name = name;
            cc.KeySequence = keys.Clone() as CheatCodeKey[];
            _CheatCodes.Add(cc);
            _Callbacks.Add(cb);
        }
    }
}
