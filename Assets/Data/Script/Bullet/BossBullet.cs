using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Contra.Bullets
{
    public class BossBullet : Bullet
    {
        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                EffectManager.Inst.PlayEffect("Boom0", transform.position);
                Destroy(gameObject);
            }
            else if (collision.gameObject.layer == LayerMask.NameToLayer("Player0"))
                Destroy(gameObject);
        }
    }
}
