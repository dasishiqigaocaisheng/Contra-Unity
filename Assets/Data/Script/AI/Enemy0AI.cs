using UnityEngine;
using Mirror;
using Contra.Network;

namespace Contra.AI
{
    public class Enemy0AI : MonoBehaviour
    {
        public float Speed;

        private Vector3 _Dir;

        private bool _IsOnGround;

        private bool _IsOnBank;

        private bool _IsNearEdge;

        private bool _IsDying;

        private Rigidbody2D _Rgd2D;

        private Collider2D _FootCldr2D;

        private Collider2D _Cldr2D;

        private Animator _Anim;

        private NetworkAnimator _NetAnim;


        private void Awake()
        {
            _Rgd2D = GetComponent<Rigidbody2D>();
            _FootCldr2D = transform.Find("Foot").GetComponent<Collider2D>();
            _Cldr2D = GetComponent<Collider2D>();
            _Anim = GetComponent<Animator>();
            _NetAnim = GetComponent<NetworkAnimator>();
        }

        // Start is called before the first frame update
        void Start()
        {
            _Dir = Player.P1.transform.position.x > transform.position.x ? Vector3.right : Vector3.left;
            if (_Dir.x > 0)
                transform.eulerAngles = Vector3.up * 180;
        }

        void FixedUpdate()
        {
            if (!NetworkServer.active)
                return;

            if (_IsDying)
                return;

            if (transform.position.y < -25)
            {
                NetworkServer.Destroy(gameObject);
                return;
            }

            _IsOnGround = _FootCldr2D.IsTouchingLayers(LayerMask.GetMask("Ground"));
            _IsOnBank = _FootCldr2D.IsTouchingLayers(LayerMask.GetMask("Bank"));

            if (_IsOnGround)
            {
                Ray2D r2d = new Ray2D(transform.position + _Dir + Vector3.up * 0.5f, Vector2.down);
                RaycastHit2D rh2d = Physics2D.Raycast(r2d.origin, r2d.direction, 0.5f, 1 << LayerMask.NameToLayer("Ground"));
                _IsNearEdge = rh2d.collider == null;
            }
            else if (_IsOnBank)
            {
                _Rgd2D.simulated = false;
                transform.position += Vector3.down * 2;
                _NetAnim.SetTrigger("FallingInWater");
            }
            else
                _IsNearEdge = false;

            Vector2 vel = _Rgd2D.velocity;

            if (_IsOnGround)
            {
                vel.x = _Dir.x * Speed;
                if (_IsNearEdge)
                {
                    vel.y = GlobalData.Inst.EnemyJumpSpeed;
                    vel.x *= 0.5f;
                }
            }

            _Rgd2D.velocity = vel;

            _Anim.SetBool("IsOnGround", _IsOnGround);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (NetworkServer.active && collision.gameObject.layer == LayerMask.NameToLayer("PlayerBullet"))
            {
                _IsDying = true;
                _Cldr2D.enabled = false;
                _Rgd2D.velocity = -_Dir * 5 + Vector3.up * 8;
                NetPlayer.P1.PlaySound(SoundManager.SoundType.Effect, "Effect0", false);
                _NetAnim.SetTrigger("Dead");
            }
        }

        private void _OnFallingInWaterAnimationEnd()
        {
            if (NetworkServer.active)
                NetworkServer.Destroy(gameObject);
        }

        private void _OnDeadAnimationEnd()
        {
            if (NetworkServer.active)
            {
                NetworkServer.Destroy(gameObject);
                EffectManager.Inst.PlayEffect("Boom1", transform.position + Vector3.up * 1.5f);
            }
        }
    }
}
