using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Modules.MessageSystem;
using UnityEngine.Animations;

namespace Contra.Animation
{
    public class OnBank : StateMachineBehaviour
    {
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (stateInfo.normalizedTime < 0.5f)
                return;

            GameMessage gm = new GameMessage { Msg0 = 1 };
            MessageManager.Inst.Send(
                animator.transform.parent.GetComponent<Player>().PID == 1 ? "P1" : "P2",
                ref gm);
        }
    }
}
