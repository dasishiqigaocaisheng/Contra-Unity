using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Modules.MessageSystem;

namespace Contra.Animation
{
    public class Player1AnimationEvent : MonoBehaviour
    {
        public void OnFallingInWaterNo2KeyFrame()
        {
            //int pid = transform.parent.GetComponent<Player>().PID == 1 ? MsgID.Player0 : MsgID.Player1;

            GameMessage gm = new GameMessage { Msg0 = 2 };
            MessageManager.Inst.SendNoCache(
                transform.parent.GetComponent<Player>().PID == 1 ? "P1" : "P2",
                ref gm);
        }
    }
}
