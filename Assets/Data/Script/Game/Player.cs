using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using Mirror;
using DG.Tweening;
using Modules;
using Modules.LogSystem;
using Modules.MessageSystem;
using Modules.Utility;
using Modules.FSM;
using Contra.Network;
using static UnityEngine.Rendering.DebugUI;

namespace Contra
{
    public class Player : NetworkBehaviour
    {
        //P1的颜色（蓝）
        private static readonly Color _P1Color = new Color(64f / 255, 64f / 255, 1);

        //P2的颜色（红）
        private static readonly Color _P2Color = new Color(181f / 255, 49f / 255, 32f / 255);

        public static Player P1 { get; set; }

        public static Player P2 { get; set; }

        public NetPlayer NPlayer => PID == 1 ? NetPlayer.P1 : NetPlayer.P2;

        /// <summary>
        /// 获取另一个玩家对象
        /// </summary>
        public Player Another => P1 == this ? P2 : P1;

        /// <summary>
        /// 在X轴上，两个玩家最多可以相距多远
        /// </summary>
        //public static float MaxDistance { get; set; }

        /// <summary>
        /// 玩家的ID 
        /// </summary>
        /// <remarks>
        /// 1代表该玩家是P1，2代表该玩家是P2
        /// </remarks>
        public int PID { get; private set; }

        /// <summary>
        /// 玩家的移动方向。NetPlayer通过该属性控制玩家的移动方向。
        /// </summary>
        public Vector2 MoveValue { get; set; }

        private int _LifeCount = 3;
        /// <summary>
        /// 玩家还剩多少条命
        /// </summary>
        public int LifeCount
        {
            get => _LifeCount;
            set
            {
                _LifeCount = value;
                GameUI.Inst.SetLifeCount(PID, value);
            }
        }

        /// <summary>
        /// 玩家是否已经处于死亡状态
        /// </summary>
        public bool IsDead { get; private set; }

        /// <summary>
        /// 玩家是否可以重生（当玩家死亡时，不能立即重生或者借命）
        /// </summary>
        public bool CanRebirth { get; private set; }

        private bool _Flash;
        /// <summary>
        /// 玩家的Sprite是否是闪烁状态
        /// </summary>
        public bool Flash
        {
            get => _Flash;
            set
            {
                GetComponentInChildren<SpriteRenderer>().material.SetFloat("_Flash", value ? 1.0f : 0);
                _Flash = value;
            }
        }


        private bool _JumpTrigger;
        public bool JumpTrigger
        {
            get
            {
                bool temp = _JumpTrigger;
                _JumpTrigger = false;
                return temp;
            }
            set => _JumpTrigger = value;
        }


        private bool _FireTrigger;
        public bool FireTrigger
        {
            get
            {
                bool temp = _FireTrigger;
                _FireTrigger = false;
                return temp;
            }
            set => _FireTrigger = value;
        }


        public Gun Gun;

        public float Speed;

        public Vector2 FirePosHorizontal;

        public Vector2 FirePosHorizontalDown;

        public Vector2 FirePosVertical;

        public Vector2 FirePosSloping;

        public Vector2 FirePosSlopingDown;

        public bool IsOnGround { get; private set; }

        public bool IsInWater { get; private set; }

        public bool IsOnBank { get; private set; }

        //挂起一次跳跃标志
        //当JumpTrigger被检测到时，会重新设置角色的y轴速度
        //但是直到下一个物理帧，角色才会与地面脱离接触。所以直到下一个物理帧角色才真正处于跳跃状态。
        private bool _JumpingPending;

        /// <summary>
        /// 这个标志表明角色是否处于跳跃状态
        /// </summary>
        public bool IsJumping { get; private set; }

        private bool _Invincible;
        /// <summary>
        /// 角色是否处于无敌状态
        /// </summary>
        public bool Invincible
        {
            get => _Invincible;
            set
            {
                _Invincible = value;
                Flash = value;
            }
        }

        private Animator _Anim;

        private NetworkAnimator _NetAnim;

        private Rigidbody2D _Rgdb;

        private BoxCollider2D _FootCollider;

        //正常站立时的碰撞体
        private CapsuleCollider2D _BodyCollider0;

        //跳跃时的碰撞体
        private CircleCollider2D _BodyCollider1;

        //卧倒时的碰撞体
        private CapsuleCollider2D _BodyCollider2;

        //潜入水中的碰撞体
        private CircleCollider2D _BodyCollider3;

        private FSM_Controller _FSMC;

        private Vector2 _VelocityVec;


        private void Awake()
        {
            _Anim = transform.Find("Sprite").GetComponent<Animator>();
            _NetAnim = transform.Find("Sprite").GetComponent<NetworkAnimator>();
            _Rgdb = GetComponent<Rigidbody2D>();
            _FootCollider = transform.Find("Foot").GetComponent<BoxCollider2D>();

            CircleCollider2D[] cldrs0 = GetComponents<CircleCollider2D>();
            _BodyCollider1 = cldrs0.First(x => x.offset.y < 2f);
            _BodyCollider3 = cldrs0.First(x => x != _BodyCollider1);

            CapsuleCollider2D[] cldrs1 = GetComponents<CapsuleCollider2D>();
            _BodyCollider0 = cldrs1.First(x => x.direction == CapsuleDirection2D.Vertical);
            _BodyCollider2 = cldrs1.First(x => x.direction == CapsuleDirection2D.Horizontal);

            if (P1 == null)
            {
                P1 = this;
                PID = 1;
                _FootCollider.gameObject.layer = LayerMask.NameToLayer("Player0Foot");
            }
            else
            {
                P2 = this;
                PID = 2;
                _FootCollider.gameObject.layer = LayerMask.NameToLayer("Player1Foot");
            }

            _Rgdb.simulated = NetworkServer.active;
        }

        void Start()
        {
            if (P1 == this)
            {
                GetComponentInChildren<SpriteRenderer>().material.SetColor("_Color0", _P1Color);
                MessageManager.Inst.Regist("P1");
            }
            else if (P2 == this)
            {
                GetComponentInChildren<SpriteRenderer>().material.SetColor("_Color0", _P2Color);
                MessageManager.Inst.Regist("P2");
            }

            _FSMC = new FSM_Controller();
            FSM fsm = _FSMC.Root_FSM;
            fsm.AddState("Common",
                fixedupdate: (x) =>
                {
                    _VelocityVec = _Rgdb.velocity;

                    //计算速度
                    if (MoveValue.x > 0.1f)
                    {
                        transform.eulerAngles = Vector3.zero;
                        _VelocityVec.x = Speed;
                    }
                    else if (MoveValue.x < -0.1f)
                    {
                        transform.eulerAngles = Vector3.up * 180;
                        _VelocityVec.x = -Speed;
                    }
                    else
                        _VelocityVec.x = 0;

                    //跳跃检测
                    if (JumpTrigger && IsOnGround)
                    {
                        if (MoveValue.y < -0.1f && transform.position.y >= -11)
                        {
                            GameMessage gm = new GameMessage { Type = 0, Msg0 = PID };
                            MessageManager.Inst.Send("Map", ref gm);
                        }
                        else
                        {
                            _VelocityVec.y = GlobalData.Inst.PlayerJumpSpeed;
                            _JumpingPending = true;
                            _AnimTrigger("Jump");
                        }
                    }

                    /*if (GameManager.Inst.IsTwoPlayers)
                    {
                        if ((transform.position - Another.transform.position).x > MaxDistance && _VelocityVec.x > 0)
                            _VelocityVec.x = 0;
                    }*/
                    _Rgdb.velocity = _VelocityVec;

                    Vector2 face_to;
                    //计算朝向
                    if (MoveValue.x != 0)
                        face_to = MoveValue;
                    else if (MoveValue.y > 0.5f)
                        face_to = Vector2.up;
                    else
                        face_to = transform.eulerAngles.y < 90 ? Vector2.right : Vector2.left;

                    //开火
                    if (FireTrigger)
                    {
                        //计算开火位置
                        Vector2 fire_pos;
                        if (!IsInWater)
                        {
                            if (MoveValue.y > 0.5f && MoveValue.x.FEqual(0))
                                fire_pos = FirePosVertical;
                            else if (MoveValue.y < -0.5f && MoveValue.x.FEqual(0))
                                fire_pos = FirePosHorizontalDown;
                            else if (MoveValue.y > 0.5f && MoveValue.x.Abs() > 0.5f)
                                fire_pos = FirePosSloping;
                            else if (MoveValue.y < -0.5f && MoveValue.x.Abs() > 0.5f)
                                fire_pos = FirePosSlopingDown;
                            else
                                fire_pos = FirePosHorizontal;
                        }
                        else
                        {
                            if (MoveValue.y > 0.5f && MoveValue.x.FEqual(0))
                                fire_pos = new Vector2(0.41f, 5.2f);
                            else if (MoveValue.y > 0.5f && MoveValue.x.Abs() > 0.5f)
                                fire_pos = new Vector2(1.35f, 3.73f);
                            else if (MoveValue.y.Abs() < 0.5f)
                                fire_pos = new Vector2(1.7f, 2.13f);
                            else
                                return;
                        }

                        fire_pos.x *= transform.eulerAngles.y < 90 ? 1 : -1;

                        if (Gun.Fire((Vector2)transform.position + fire_pos, face_to))
                            _NetAnim.SetTrigger("Fire");
                    }
                });
            fsm.AddState("FallingInWater");
            fsm.AddState("Banking");

            fsm.SetBeginPath("Common",
                cond: (x) => GameManager.Inst.Started,
                transfer: (x) =>
                {
                    _Anim.SetBool("IsOnGround", false);
                    _Anim.SetBool("IsInWater", false);
                    _AnimTrigger("Jump");
                    IsOnGround = false;
                    IsInWater = false;
                    IsOnBank = false;
                    Gun.GunType = Gun.Type.Normal;
                    _BodyCollider1.enabled = true;

                    if (NetworkServer.active)
                    {
                        NPlayer.SetInvincible(true);
                        DOVirtual.DelayedCall(3, () => NPlayer.SetInvincible(false)).SetId(this);
                    }
                });
            fsm.AddTransferPath("Common", "FallingInWater",
                cond: (x) => IsOnBank && !IsOnGround && !IsInWater,
                transfer: (x) =>
                {
                    _AnimTrigger("FallingInWater");
                    _Rgdb.velocity = Vector2.zero;
                });
            fsm.AddTransferPath("FallingInWater", "Common",
                cond: (x) => IsInWater);

            fsm.AddTransferPath("Common", "Banking",
                cond: (x) => IsOnBank && IsInWater,
                transfer: (x) =>
                {
                    _AnimTrigger("OnBank");
                    _Rgdb.velocity = Vector2.zero;
                });
            fsm.AddTransferPath("Banking", "Common",
                cond: (x) => IsOnGround);

            //int id = PID == 1 ? MsgID.Player0 : MsgID.Player1;
            //Msg0==1，上岸
            MessageManager.Inst.CreateFilter(PID == 1 ? "P1" : "P2",
                (in GameMessage x) => x.Msg0.Int == 1,
                (in GameMessage x) =>
                {
                    if (!NetworkServer.active)
                        return;

                    IsOnBank = false;
                    if (transform.forward.z > 0f)
                        _Rgdb.position += new Vector2(1f, 2.4f);
                    else
                        _Rgdb.position += new Vector2(-1f, 2.4f);
                });

            //Msg0==2，跳水动画第2个关键帧
            MessageManager.Inst.CreateFilter(PID == 1 ? "P1" : "P2",
                (in GameMessage x) => x.Msg0.Int == 2,
                (in GameMessage x) =>
                {
                    if (!NetworkServer.active)
                        return;

                    _Rgdb.velocity = Vector2.zero;
                    _Rgdb.position += Vector2.down;
                });

            GameUI.Inst.SetLifeCount(PID, LifeCount);

            if (NetworkServer.active)
                _FSMC.Run();
        }

        void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                IsOnGround = _FootCollider.IsTouchingLayers(1 << LayerMask.NameToLayer("Ground"));
                IsInWater = _FootCollider.IsTouchingLayers(1 << LayerMask.NameToLayer("WaterGround"));
                IsOnBank = _FootCollider.IsTouchingLayers(1 << LayerMask.NameToLayer("Bank"));

                if (_JumpingPending && !IsOnGround && !IsInWater && !IsOnBank)
                {
                    IsJumping = true;
                    _JumpingPending = false;
                }
                else if (IsOnGround || IsInWater || IsOnBank)
                {
                    if (IsOnGround && IsJumping)
                        NPlayer.PlaySound(SoundManager.SoundType.Other, "Foot", false);

                    IsJumping = false;
                }

                (_BodyCollider0.enabled, _BodyCollider1.enabled, _BodyCollider2.enabled, _BodyCollider3.enabled) = (IsOnGround, IsInWater, IsJumping) switch
                {
                    //在地面上时
                    (true, _, _) => (MoveValue.y > -0.5f, false, MoveValue.y <= -0.5f, false),
                    //自由落体时
                    (false, false, false) => (true, false, false, false),
                    //在水中时
                    (_, true, _) => (false, false, false, MoveValue.y > -0.5f),
                    _ => (false, IsJumping, false, false)
                };

                //下落死亡
                if (_FootCollider.transform.position.y < -15 && !IsDead)
                    _Dead();

                _Anim.SetFloat("View", MoveValue.y);
                _Anim.SetFloat("Speed", Mathf.Abs(_VelocityVec.x / Speed));
                _Anim.SetBool("IsOnGround", IsOnGround);
                _Anim.SetBool("IsInWater", IsInWater);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (IsDead)
                return;
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ammo"))
            {
                Gun.GunType = Enum.Parse<Gun.Type>(collision.gameObject.name);
                NetPlayer.P1.PlaySound(SoundManager.SoundType.Other, "Effect2", false);
                Destroy(collision.gameObject);
            }
            else if (collision.gameObject.layer == LayerMask.NameToLayer("EnemyBullet") ||
                    (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") && collision.gameObject.CompareTag("AggressiveEnemy")))
            {
                if (Invincible)
                    return;
                _Dead();
            }
        }

        public static Player ClosestAliveOne(Vector3 pos)
        {
            if (GameManager.Inst.IsTwoPlayers)
            {
                float dist0 = (P1.transform.position - pos).x.Abs();
                if ((P2.transform.position - pos).x.Abs() >= dist0)
                    return P1.IsDead ? P2 : P1;
                else
                    return P2.IsDead ? P1 : P2;
            }
            else
                return P1;
        }

        [Server]
        private void _Rebirth()
        {
            //找到一块可以出生的地方
            float x = GameObject.Find("LeftWall").transform.position.x + 2;
            RaycastHit2D rh2d = new RaycastHit2D();
            while (rh2d.rigidbody == null)
                rh2d = Physics2D.Raycast(new Vector2(x++, 20), Vector2.down, 100, LayerMask.GetMask("Ground"));

            //重生
            NPlayer.SetPlayerActive(true);
            transform.position = new Vector3(rh2d.point.x, 20);
            _Rgdb.velocity = Vector2.zero;
            _Anim.SetBool("Dead", false);
            _FSMC.Run();

            IsDead = false;
        }

        [ClientRpc]
        public void BorrowLife()
        {
            //只有没有生命的时候才可以借命
            if (NetworkServer.active && LifeCount < 0)
                _Rebirth();
            LifeCount++;
            GameUI.Inst.SetLifeCount(Another.PID, --Another.LifeCount);
            GameUI.Inst.SetLifeCount(PID, LifeCount);
        }

        [Server]
        private void _Dead()
        {
            if (IsDead)
                return;

            _Anim.SetBool("Dead", true);
            _Rgdb.velocity = new Vector2(-3, 25);
            _BodyCollider0.enabled = false;
            _BodyCollider1.enabled = false;
            _BodyCollider2.enabled = false;
            _BodyCollider3.enabled = false;
            IsDead = true;
            CanRebirth = false;
            _FSMC.Stop();
            _LostLife();
            NetPlayer.P1.PlaySound(SoundManager.SoundType.Other, "Dead", false);

            //死亡后3s内无法借命
            DOVirtual.DelayedCall(3, () =>
            {
                if (LifeCount >= 0)
                    _Rebirth();
                else
                {
                    if (!GameManager.Inst.IsTwoPlayers || Another.LifeCount < 0)
                        NetPlayer.P1.GameEnd(false);
                    NPlayer.SetPlayerActive(false);
                }
                CanRebirth = true;
            }).SetId(this);
        }

        [ClientRpc]
        private void _AnimTrigger(string name)
        {
            _Anim.SetTrigger(name);
        }

        [ClientRpc]
        private void _LostLife()
        {
            //if (--LifeCount <= 3)
            LifeCount--;
            GameUI.Inst.SetLifeCount(PID, LifeCount);
        }

        [Server]
        public void GotoExitPos(Vector3 pos)
        {
            StartCoroutine(_GotoExitPos_Coroutine(pos));
        }

        private IEnumerator _GotoExitPos_Coroutine(Vector3 pos)
        {
            MoveValue = Vector2.right;
            while (transform.position.x < pos.x)
            {
                if (IsOnGround)
                {
                    RaycastHit2D rh2d = Physics2D.Raycast((Vector2)transform.position + new Vector2(1, 0.5f), Vector2.down, 1, LayerMask.GetMask("Ground"));
                    if (rh2d.collider == null)
                        JumpTrigger = true;
                }
                yield return null;
            }

            while (!IsOnGround)
                yield return null;
            JumpTrigger = true;

            yield return new WaitForSeconds(4);
            NetPlayer.P1.GameEnd(true);

            _Rgdb.simulated = false;
        }

        private void OnDestroy()
        {
            DOTween.Kill(this);
            if (FSMManager.Inst != null)
                FSMManager.Inst.UnRegistFSMC(_FSMC);

        }
    }
}
