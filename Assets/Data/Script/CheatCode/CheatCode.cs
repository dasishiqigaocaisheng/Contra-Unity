
namespace Contra.CheatCode
{
    internal class CheatCode
    {
        public string Name { get; set; }

        public CheatCodeKey[] KeySequence { get; set; }

        private int _IdxReg0;

        private int _IdxReg1;

        public bool CheckOnce(Player ch, CheatCodeKey key)
        {
            if (KeySequence == null || KeySequence.Length == 0)
                return false;

            if (ch == Player.P1)
            {
                if (KeySequence[_IdxReg0] == key)
                    _IdxReg0++;
                else
                    _IdxReg0 = 0;
                return _IdxReg0 == KeySequence.Length;
            }
            else
            {
                if (KeySequence[_IdxReg1] == key)
                    _IdxReg1++;
                else
                    _IdxReg1 = 0;
                return _IdxReg1 == KeySequence.Length;
            }

            //return _IdxReg == KeySequence.Length;
        }

        public void Reset(Player ch)
        {
            if (ch == Player.P1)
                _IdxReg0 = 0;
            else
                _IdxReg1 = 0;
        }
    }
}
