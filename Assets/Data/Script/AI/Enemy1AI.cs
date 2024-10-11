using UnityEngine;
using DG.Tweening;
using Mirror;
using Modules.Utility;
using Contra.Bullets;
using Contra.Network;

namespace Contra.AI
{
    public class Enemy1AI : MonoBehaviour
    {
        public Vector3 FirePos;

        private Collider2D _Cldr2D;

        private Rigidbody2D _Rdg2D;

        private Animator _Anim;

        private NetworkAnimator _NetAnim;

        private bool _IsVisible;

        private bool _IsInCD;

        private bool _IsDying;


        private void Awake()
        {
            _Anim = GetComponent<Animator>();
            _NetAnim = GetComponent<NetworkAnimator>();
            _Cldr2D = GetComponent<Collider2D>();
            _Rdg2D = GetComponent<Rigidbody2D>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!NetworkServer.active)
                return;

            if (_IsDying)
                return;

            Player target = Player.ClosestAliveOne(transform.position);

            if (target.transform.position.x - transform.position.x > 0)
                transform.eulerAngles = Vector3.up * 180;
            else
                transform.eulerAngles = Vector3.up;

            if (!_IsInCD)
            {
                if (!_IsVisible || Mathf.Abs(target.transform.position.x - transform.position.x) >= 20)
                    return;

                _IsInCD = true;
                _Cldr2D.enabled = true;
                _Anim.SetBool("IsAppearing", true);

                DOVirtual.DelayedCall(1, () =>
                {
                    Vector3 dir = (target.transform.position - transform.position).Proj2X().normalized;
                    dir = dir == Vector3.zero ? Vector3.left : dir;

                    Bullet.Spawn("Enemy", transform.position + new Vector3(FirePos.x * -dir.x, FirePos.y), dir);
                }).SetLink(gameObject);
                DOVirtual.DelayedCall(2, () =>
                {
                    _Cldr2D.enabled = false;
                    _Anim.SetBool("IsAppearing", false);
                    DOVirtual.DelayedCall(2, () => _IsInCD = false);
                }).SetLink(gameObject);
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
                _Rdg2D.velocity = Vector3.up * 6f;
                _Rdg2D.gravityScale = 1;
                NetPlayer.P1.PlaySound(SoundManager.SoundType.Effect, "Effect0", false);
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
