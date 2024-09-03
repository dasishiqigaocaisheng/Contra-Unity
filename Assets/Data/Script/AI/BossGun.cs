using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Contra.Bullets;
using Contra.Network;

namespace Contra.AI
{
    public class BossGun : NetworkBehaviour
    {
        public float FirePosOffset;

        private Animator _Anim;

        private int _HP;

        private bool _IsBroken;


        private void Awake()
        {
            _Anim = GetComponent<Animator>();
        }

        private void Start()
        {
            _HP = GlobalData.Inst.BossGunHP;
        }

        public void Fire()
        {
            if (_IsBroken)
                return;

            Vector3 v = new Vector3(-Random.Range(3f, 6f), Random.Range(2f, 4f), 0);
            Bullet.Spawn("Boss", transform.position + Vector3.left * FirePosOffset, v);
            _Anim.SetTrigger("Fire");
        }

        public void Broken()
        {
            if (_IsBroken)
                return;

            _IsBroken = true;
            EffectManager.Inst.PlayEffect("Boom0", transform.position + Vector3.left * FirePosOffset * 0.5f);
            GetComponent<BoxCollider2D>().enabled = false;
            _Anim.SetBool("Broken", true);
        }

        [ClientRpc]
        private void _AnimTrigger(string name)
        {
            _Anim.SetTrigger(name);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!NetworkServer.active || _IsBroken)
                return;

            if (collision.gameObject.layer == LayerMask.NameToLayer("PlayerBullet"))
            {
                if (--_HP == 0)
                {
                    Broken();
                    NetPlayer.P1.PlaySound(SoundManager.SoundType.Effect, "Effect3", false);
                }
                else
                    NetPlayer.P1.PlaySound(SoundManager.SoundType.Effect, "Effect1", false);

            }
        }
    }
}
