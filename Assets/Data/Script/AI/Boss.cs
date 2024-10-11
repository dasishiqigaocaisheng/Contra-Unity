using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Modules.FSM;
using Modules.LogSystem;
using Modules.MessageSystem;
using Mirror;
using Contra.Network;

namespace Contra.AI
{
    public class Boss : MonoBehaviour
    {
        [SerializeField]
        private Vector2 _LeftFirePos;

        [SerializeField]
        private Vector2 _RightFirePos;

        private FSM_Controller _FSMC;

        private Animator _Anim;

        private int _HP;

        private BossGun _LeftGun;

        private BossGun _RightGun;


        private void Awake()
        {
            _Anim = GetComponent<Animator>();
            _LeftGun = transform.Find("LeftGun").GetComponent<BossGun>();
            _RightGun = transform.Find("RightGun").GetComponent<BossGun>();
        }

        // Start is called before the first frame update
        void Start()
        {
            _FSMC = FSMManager.Inst.NewFSMC();

            FSM fsm = _FSMC.Root_FSM;
            fsm.AddState("Sleep");
            fsm.AddState("LeftFire",
                enter: (x) =>
                {
                    x.Attached_FSM.Controller.Block();
                    DOVirtual.DelayedCall(1, () => x.Attached_FSM.Controller.CancelBlock());
                },
                exit: (x) => _LeftGun.Fire());
            fsm.AddState("RightFire",
                enter: (x) =>
                {
                    x.Attached_FSM.Controller.Block();
                    DOVirtual.DelayedCall(1, () => x.Attached_FSM.Controller.CancelBlock());
                },
                exit: (x) => _RightGun.Fire());
            fsm.AddState("Broken",
                enter: (x) =>
                {
                    x.Attached_FSM.Controller.Block();

                    Sequence sq = DOTween.Sequence();
                    sq.Append(DOVirtual.DelayedCall(0.3f, () =>
                    {
                        Transform brk0 = transform.Find("Broken0");
                        Transform brk1 = transform.Find("Broken1");

                        brk0.gameObject.SetActive(true);
                        brk1.gameObject.SetActive(true);
                        EffectManager.Inst.PlayEffect("Boom0", brk0.transform.position);
                        EffectManager.Inst.PlayEffect("Boom0", brk1.transform.position);
                    }));
                    sq.Append(DOVirtual.DelayedCall(0.3f, () =>
                    {
                        Transform brk0 = transform.Find("Broken2");
                        Transform brk1 = transform.Find("Broken3");

                        brk0.gameObject.SetActive(true);
                        brk1.gameObject.SetActive(true);
                        EffectManager.Inst.PlayEffect("Boom0", brk0.transform.position);
                        EffectManager.Inst.PlayEffect("Boom0", brk1.transform.position);
                    }));
                    sq.Append(DOVirtual.DelayedCall(0.3f, () =>
                    {
                        Transform brk0 = transform.Find("Broken4");
                        Transform brk1 = transform.Find("Broken5");

                        brk0.gameObject.SetActive(true);
                        brk1.gameObject.SetActive(true);
                        EffectManager.Inst.PlayEffect("Boom0", brk0.transform.position);
                        EffectManager.Inst.PlayEffect("Boom0", brk1.transform.position);
                    }));
                    sq.Append(DOVirtual.DelayedCall(0.3f, () =>
                    {
                        Transform brk0 = transform.Find("Broken6");
                        Transform brk1 = transform.Find("Broken7");

                        brk0.gameObject.SetActive(true);
                        brk1.gameObject.SetActive(true);
                        EffectManager.Inst.PlayEffect("Boom0", brk0.transform.position);
                        EffectManager.Inst.PlayEffect("Boom0", brk1.transform.position);
                    }));
                    sq.Append(DOVirtual.DelayedCall(0.5f, null));
                    sq.onComplete = () => _FSMC.CancelBlock();

                    _LeftGun.Broken();
                    _RightGun.Broken();

                    NetPlayer.P1.PlaySound(SoundManager.SoundType.Effect, "Effect4", false);
                    //_Anim.SetTrigger("Broken");
                });
            fsm.AddState("Dead",
                enter: (x) =>
                {
                    transform.Find("Broken0").gameObject.SetActive(false);
                    transform.Find("Broken1").gameObject.SetActive(false);
                    transform.Find("Broken2").gameObject.SetActive(false);
                    transform.Find("Broken3").gameObject.SetActive(false);
                    transform.Find("Broken4").gameObject.SetActive(false);
                    transform.Find("Broken5").gameObject.SetActive(false);
                    transform.Find("Broken6").gameObject.SetActive(false);
                    transform.Find("Broken7").gameObject.SetActive(false);

                    GetComponent<SpriteRenderer>().material.SetFloat("_Bypass", 1);
                    transform.Find("AirWall").gameObject.SetActive(false);

                    _Anim.SetBool("Dead", true);

                    GameMessage gm = new GameMessage { Type = 2 };
                    MessageManager.Inst.Send("GameManager", ref gm);
                });

            fsm.SetBeginPath("Sleep", (x) => GameManager.Inst.Started);
            fsm.AddTransferPath("Sleep", "LeftFire",
                cond: (x) => transform.position.x - Player.ClosestAliveOne(transform.position).transform.position.x < 30,
                transfer: (x) => NetPlayer.P1.PlaySound(SoundManager.SoundType.Effect, "Boss", false));
            fsm.AddTransferPath("LeftFire", "RightFire", FSM.ALWAYS);
            fsm.AddTransferPath("RightFire", "LeftFire", FSM.ALWAYS);
            fsm.AddTransferPath("LeftFire", "Broken", FSM.MANUAL);
            fsm.AddTransferPath("RightFire", "Broken", FSM.MANUAL);
            fsm.AddTransferPath("Broken", "Dead", FSM.ALWAYS);

            if (NetworkServer.active)
                _FSMC.Run();

            _HP = GlobalData.Inst.BossHP;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("PlayerBullet"))
            {
                NetPlayer.P1.PlaySound(SoundManager.SoundType.Effect, "Effect1", false);
                if (--_HP == 0)
                    _FSMC.StateTransfer("Broken");
            }
        }

        private void OnDestroy()
        {
            if (FSMManager.Inst != null)
                FSMManager.Inst.UnRegistFSMC(_FSMC);
        }
    }
}
