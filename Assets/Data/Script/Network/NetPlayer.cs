using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using Modules.MessageSystem;
using Contra.CheatCode;

namespace Contra.Network
{
    //Host通过操作NetPlayer来同步操控各个Client上的Player
    //因此NetPlayer实际上起到Host与各个Client之间的中介作用
    [Serializable]
    public class NetPlayer : NetworkBehaviour
    {
        [SerializeField]
        private int _PID;
        /// <summary>
        /// Player的ID，Host为1，Client为2
        /// </summary>
        public int PID { get => _PID; private set => _PID = value; }

        public static NetPlayer P1 { get; private set; }

        public static NetPlayer P2 { get; private set; }

        /// <summary>
        /// 此Player所代表的角色
        /// </summary>
        public Player ThisPlayer => PID == 1 ? Player.P1 : Player.P2;

        [SyncVar]
        private bool _PlayerControl;
        /// <summary>
        /// 玩家是否可以控制自己的角色
        /// </summary>
        public bool PlayerControl { get => _PlayerControl; set => _PlayerControl = value; }

        private PlayerInput _Input;



        private void Awake()
        {
            _Input = GetComponent<PlayerInput>();
        }

        private void Start()
        {
            if (NetworkServer.active)
            {
                if (isLocalPlayer)
                {
                    PID = 1;
                    P1 = this;
                }
                else
                {
                    PID = 2;
                    P2 = this;
                }
            }
            else if (NetworkClient.active)
            {
                if (isLocalPlayer)
                {
                    PID = 2;
                    P2 = this;
                }
                else
                {
                    PID = 1;
                    P1 = this;
                }
            }
        }

        /// <summary>
        /// 通知游戏已经开始
        /// </summary>
        [ClientRpc]
        public void Started()
        {
            GameManager.Inst.Started = true;
        }

        /// <summary>
        /// 通知游戏结束
        /// </summary>
        /// <param name="is_success">是否通关成功</param>
        [ClientRpc]
        public void GameEnd(bool is_success)
        {
            GameUI.Inst.GameEndUI(is_success);
            GameManager.Inst.GameEnd = true;
            if (!is_success)
                SoundManager.Inst.Play(SoundManager.SoundType.BGM, "Fail", false);
        }

        /// <summary>
        /// 设置该Player的ID
        /// </summary>
        [ClientRpc]
        public void SetPID(int pid)
        {
            PID = pid;
            if (pid == 1)
            {
                P1 = this;
                _Input.SwitchCurrentActionMap("Player1");
            }
            else if (pid == 2)
            {
                P2 = this;
                _Input.SwitchCurrentActionMap("Player2");
            }
        }

        /// <summary>
        /// 设置该Player的生命数量
        /// </summary>
        /// <param name="count"></param>
        [Command]
        public void SetLifeCount(int count)
        {
            _SetLifCount(count);
        }

        [ClientRpc]
        void _SetLifCount(int count)
        {
            ThisPlayer.LifeCount = count;
        }

        /// <summary>
        /// 输出一条消息（在屏幕左下角）
        /// </summary>
        /// <param name="info"></param>
        [Command]
        public void GameUIInfo(string info)
        {
            _GameUIInfo(info);
        }

        [ClientRpc]
        void _GameUIInfo(string info)
        {
            GameUI.Inst.Info(info);
        }

        /// <summary>
        /// 播放声音
        /// </summary>
        /// <param name="type">声音类型</param>
        /// <param name="name">声音名称</param>
        /// <param name="loop">是否循环播放</param>
        [ClientRpc]
        public void PlaySound(SoundManager.SoundType type, string name, bool loop)
        {
            SoundManager.Inst.Play(type, name, loop);
        }

        /// <summary>
        /// 设置Player是否为无敌状态
        /// </summary>
        [ClientRpc]
        public void SetInvincible(bool value)
        {
            ThisPlayer.Invincible = value;
        }

        /// <summary>
        /// 设置Player是否处于闪烁状态
        /// </summary>
        /// <param name="value"></param>
        [ClientRpc]
        public void SetFlash(bool value)
        {
            ThisPlayer.Flash = value;
        }

        /// <summary>
        /// 设置Player是否处于Active状态
        /// </summary>
        /// <param name="value"></param>
        [ClientRpc]
        public void SetPlayerActive(bool value)
        {
            ThisPlayer.gameObject.SetActive(value);
        }

        public void _OnMove(InputAction.CallbackContext cbc)
        {
            if (isOwned && PlayerControl)
            {
                if (cbc.performed)
                    _OnMoveCmd(cbc.ReadValue<Vector2>());
                else if (cbc.canceled)
                    _OnMoveCmd(Vector2.zero);
            }
        }

        public void _OnJump(InputAction.CallbackContext cbc)
        {
            if (isOwned && PlayerControl && cbc.performed)
                _OnJumpCmd(true);
        }

        public void _OnFire(InputAction.CallbackContext cbc)
        {
            if (isOwned && cbc.performed)
            {
                if (PlayerControl)
                    _OnFireCmd(true);
                if (GameManager.Inst.GameEnd)
                {
                    GameMessage gm = new GameMessage { Type = 3 };
                    MessageManager.Inst.Send("GameManager", ref gm);
                    PlayerControl = false;
                }
            }
        }

        [Command]
        private void _OnMoveCmd(Vector2 value)
        {
            if (value == Vector2.up)
                CheatCodeManager.Push(ThisPlayer, CheatCodeKey.Up);
            else if (value == Vector2.down)
                CheatCodeManager.Push(ThisPlayer, CheatCodeKey.Down);
            else if (value == Vector2.left)
                CheatCodeManager.Push(ThisPlayer, CheatCodeKey.Left);
            else if (value == Vector2.right)
                CheatCodeManager.Push(ThisPlayer, CheatCodeKey.Right);

            _OnMoveRpc(value);
        }

        [ClientRpc]
        private void _OnMoveRpc(Vector2 value) => ThisPlayer.MoveValue = value;

        [Command]
        private void _OnJumpCmd(bool value)
        {
            CheatCodeManager.Push(ThisPlayer, CheatCodeKey.B);
            _OnJumpRpc(value);
        }

        [ClientRpc]
        private void _OnJumpRpc(bool value) => ThisPlayer.JumpTrigger = value;

        [Command]
        private void _OnFireCmd(bool value)
        {
            CheatCodeManager.Push(ThisPlayer, CheatCodeKey.A);
            _OnFireRpc(value);
        }

        [ClientRpc]
        private void _OnFireRpc(bool value)
        {
            if (ThisPlayer.LifeCount >= 0)
                ThisPlayer.FireTrigger = value;
            else if (GameManager.Inst.IsTwoPlayers && ThisPlayer.Another.LifeCount > 0 && ThisPlayer.CanRebirth)
                ThisPlayer.BorrowLife();
        }

    }
}
