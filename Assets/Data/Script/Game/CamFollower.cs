using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Modules.MessageSystem;

namespace Contra
{
    public class CamFollower : MonoBehaviour
    {
        void Update()
        {
            if (!GameManager.Inst.Started || !NetworkServer.active)
                return;

            Vector3 pos;
            if (GameManager.Inst.IsTwoPlayers)
            {
                if (!Player.P1.IsDead && !Player.P2.IsDead)
                    pos = (Player.P1.transform.position + Player.P2.transform.position) / 2;
                else if (Player.P1.IsDead)
                    pos = Player.P2.transform.position;
                else
                    pos = Player.P1.transform.position;
            }
            else
                pos = Player.P1.transform.position;

            //镜头只会向前移动，不会回退
            if (pos.x > transform.position.x)
                transform.position = pos;
        }
    }
}
