using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Mirror;
using Modules.Utility;
using Contra.Bullets;

namespace Contra.AI
{
    public class Enemy2AI : MonoBehaviour
    {
        private bool _IsVisible;

        private Animator _Anim;

        private NetworkAnimator _NetAnim;

        private Collider2D _Cldr2D;

        private Rigidbody2D _Rgd2D;

        private bool _IsInCD;

        private bool _IsDying;

        private Vector3 _DirQuantize;


        private void Awake()
        {
            _Anim = GetComponent<Animator>();
            _NetAnim = GetComponent<NetworkAnimator>();
            _Cldr2D = GetComponent<Collider2D>();
            _Rgd2D = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (!NetworkServer.active)
                return;

            if (!_IsVisible || _IsDying)
                return;

            Vector3 dir = Player.ClosestAliveOne(transform.position).transform.position - transform.position;

            if (!_IsInCD)
            {
                Vector3 firepos = dir.x > 0 ? Vector3.right : Vector3.left;
                if (_DirQuantize.y == 1)
                    firepos.y = 4.8f;
                else if (_DirQuantize.y == 0)
                    firepos.y = 3.4f;
                else if (_DirQuantize.y == -1)
                    firepos.y = 2;
                firepos += transform.position;

                Bullet.Spawn("Enemy", firepos, dir);
                DOVirtual.DelayedCall(0.5f, () => Bullet.Spawn("Enemy", firepos, dir)).SetLink(gameObject);
                DOVirtual.DelayedCall(1.0f, () =>
                {
                    Bullet.Spawn("Enemy", firepos, dir);
                    DOVirtual.DelayedCall(3, () => _IsInCD = false);
                }).SetLink(gameObject);
                _IsInCD = true;
            }
            else
            {
                if (dir.x < 0)
                {
                    _DirQuantize.x = -1;
                    transform.eulerAngles = Vector3.zero;
                }
                else
                {
                    _DirQuantize.x = 1;
                    transform.eulerAngles = Vector3.up * 180;
                }

                if (dir.y > 0)
                {
                    if (dir.y > dir.x.Abs())
                        _DirQuantize.y = 1;
                    else
                        _DirQuantize.y = 0;
                }
                else
                {
                    if (-dir.y > dir.x.Abs())
                        _DirQuantize.y = -1;
                    else
                        _DirQuantize.y = 0;
                }

                _Anim.SetFloat("LookDir", _DirQuantize.y * 2);
            }
        }

        private void OnBecameVisible()
        {
            _IsVisible = true;
        }

        private void OnBecameInvisible()
        {
            _IsVisible = false;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (NetworkServer.active && collision.gameObject.layer == LayerMask.NameToLayer("PlayerBullet"))
            {
                _IsDying = true;
                _Cldr2D.enabled = false;
                _Rgd2D.velocity = Vector2.up * 6;
                _Rgd2D.gravityScale = 1;
                SoundManager.Inst.Play(SoundManager.SoundType.Effect, "Effect0", false);
                _NetAnim.SetTrigger("Dead");
            }
        }

        private void _OnDeadAnimationEnd()
        {
            if (NetworkServer.active)
            {
                NetworkServer.Destroy(gameObject);
                EffectManager.Inst.PlayEffect("Boom1", transform.position + Vector3.up * 2);
            }
        }
    }
}
