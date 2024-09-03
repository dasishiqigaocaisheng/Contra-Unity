using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Mirror;
using Modules.FSM;
using Modules.Utility;
using Modules.LogSystem;
using Contra.Bullets;

namespace Contra.AI
{
    public class Battery0AI : MonoBehaviour
    {
        private Animator _Anim;

        private FSM_Controller _FSMC;

        private BoxCollider2D _Cldr2D;

        private int _HP;

        private readonly Vector2[] _DirDecode = new Vector2[] {
        new Vector2(-1,0),
        new Vector2(-Mathf.Cos(Mathf.PI/6),Mathf.Sin(Mathf.PI/6)),
        new Vector2(-Mathf.Sin(Mathf.PI/6),Mathf.Cos(Mathf.PI/6)),
        new Vector2(0,1),
        new Vector2(Mathf.Sin(Mathf.PI/6),Mathf.Cos(Mathf.PI/6)),
        new Vector2(Mathf.Cos(Mathf.PI/6),Mathf.Sin(Mathf.PI/6)),
        new Vector2(1,0),
        new Vector2(Mathf.Cos(Mathf.PI/6),-Mathf.Sin(Mathf.PI/6)),
        new Vector2(Mathf.Sin(Mathf.PI/6),-Mathf.Cos(Mathf.PI/6)),
        new Vector2(0,-1),
        new Vector2(-Mathf.Sin(Mathf.PI/6),-Mathf.Cos(Mathf.PI/6)),
        new Vector2(-Mathf.Cos(Mathf.PI/6), -Mathf.Sin(Mathf.PI/6))
    };

        private void Awake()
        {
            _Anim = transform.Find("Sprite").GetComponent<Animator>();
            _Cldr2D = GetComponent<BoxCollider2D>();
        }

        private void Start()
        {
            static float __DirEncode(Vector2 v)
            {
                float dir_encode;
                float slope = v.Slope();

                //tan15°=0.26794
                //tan45°=1
                //tan75°=3.73205
                if (slope == float.NaN)
                    dir_encode = 0;
                else
                {
                    if (slope.Abs() < 0.26794f)
                        dir_encode = 6;
                    else if (slope.Abs() < 1)
                        dir_encode = 5;
                    else if (slope.Abs() < 3.73205f)
                        dir_encode = 4;
                    else
                        dir_encode = 3;
                }

                if (slope >= 0)
                {
                    if (v.x < 0)
                        dir_encode = (dir_encode + 6) % 12;
                }
                else
                {
                    if (v.x < 0)
                        dir_encode = 6 - dir_encode;
                    else if (v.y < 0)
                        dir_encode = 12 - dir_encode;
                }

                return dir_encode;
            }

            _HP = GlobalData.Inst.BatteryHP;

            _FSMC = FSMManager.Inst.NewFSMC();

            FSM fsm = _FSMC.Root_FSM;
            fsm.AddState("Wait",
                enter: (x) =>
                {
                    _Cldr2D.enabled = false;
                    _Anim.SetBool("IsOpen", false);
                },
                update: (x) =>
                {
                    if (GameManager.Inst.Started && Mathf.Abs(Player.ClosestAliveOne(transform.position).transform.position.x - transform.position.x) < 10)
                        x.GotoState("Active");
                },
                exit: (x) =>
                {
                    _Cldr2D.enabled = true;
                    _Anim.SetBool("IsOpen", true);
                    _Anim.SetFloat("Dir", 0);
                });
            fsm.AddState("Active",
                enter: (x) =>
                {
                    _FSMC.Block();
                    DOVirtual.DelayedCall(1.2f, () => _FSMC?.CancelBlock());
                },
                update: (x) =>
                {
                    Vector3 dir = Player.ClosestAliveOne(transform.position).transform.position - transform.position;

                    if (dir.x.Abs() > 10)
                        x.GotoState("Wait");
                    else
                    {
                        float dir_code_rec = _Anim.GetFloat("Dir");
                        float dir_code_exp = __DirEncode(dir);

                        if (dir_code_rec != dir_code_exp)
                        {
                            float dir_code_nxt = (dir_code_rec + 1) % 12;
                            float dir_code_pre = (dir_code_rec + 11) % 12;

                            float dist_nxt = Mathf.Min((dir_code_nxt - dir_code_exp).Abs() % 12, 12 - (dir_code_nxt - dir_code_exp).Abs() % 12);
                            float dist_pre = Mathf.Min((dir_code_pre - dir_code_exp).Abs() % 12, 12 - (dir_code_pre - dir_code_exp).Abs() % 12);

                            dir_code_rec = dist_nxt <= dist_pre ? dir_code_nxt : dir_code_pre;
                        }

                        _Anim.SetFloat("Dir", dir_code_rec);

                        if (dir_code_rec == dir_code_exp)
                            Bullet.Spawn("Enemy", transform.position + (Vector3)_DirDecode[dir_code_rec.Round()] * 1.5f, (Vector3)_DirDecode[dir_code_rec.Round()]);

                        x.GotoState("Active");
                    }
                });

            fsm.SetBeginPath("Wait", FSM.ALWAYS);
            fsm.AddTransferPath("Wait", "Active", FSM.MANUAL);
            fsm.AddTransferPath("Active", "Active", FSM.MANUAL);
            fsm.AddTransferPath("Active", "Wait", FSM.MANUAL);

            if (NetworkServer.active)
                _FSMC.Run();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (NetworkServer.active && collision.gameObject.layer == LayerMask.NameToLayer("PlayerBullet"))
            {
                _HP--;
                if (_HP == 0)
                {
                    EffectManager.Inst.PlayEffect("Boom0", transform.position);
                    SoundManager.Inst.Play(SoundManager.SoundType.Effect, "Effect3", false);
                    NetworkServer.Destroy(gameObject);
                }
                else
                    SoundManager.Inst.Play(SoundManager.SoundType.Effect, "Effect1", false);
            }
        }

        private void OnDestroy()
        {
            if (FSMManager.Inst != null)
                FSMManager.Inst.UnRegistFSMC(_FSMC);
        }
    }
}
