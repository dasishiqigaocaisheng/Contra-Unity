using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Mirror;
using Modules.FSM;
using Modules.MessageSystem;
using Modules.LogSystem;
using Modules.Utility.Singleton;
using Contra.CheatCode;
using Contra.Network;
using Contra.Bullets;

namespace Contra
{
    public class GameManager : MonoSingleton<GameManager>
    {
        [SerializeField]
        private GlobalData _GD;

        /// <summary>
        /// 游戏主相机
        /// </summary>
        public Camera MainCamera { get; private set; }

        /// <summary>
        /// 游戏是否处于Host模式
        /// </summary>
        public bool IsHostMode { get; private set; }

        /// <summary>
        /// 游戏是否处于双人模式
        /// </summary>
        public bool IsTwoPlayers { get; private set; }

        /// <summary>
        /// 本地玩家个数
        /// </summary>
        public int LocalPlayerNum { get; private set; }

        /// <summary>
        /// 游戏是否已经开始
        /// </summary>
        public bool Started { get; set; }

        /// <summary>
        /// 游戏是否已经结束（指结算界面）
        /// </summary>
        public bool GameEnd { get; set; }

        private FSM_Controller _FSMC;

        private Plane[] _FrustumPlanes = new Plane[6];

        public void _OnModuleReady()
        {
            //一开始进入游戏以后有一段音乐只会播放一次
            //然后会循环播放BGM
            IEnumerator __GameStartSoundPlay()
            {
                SoundManager.Inst.Play(SoundManager.SoundType.BGM, "GameStart", false);
                while (SoundManager.Inst.IsSourcePlaying(SoundManager.SoundType.BGM))
                    yield return null;
                SoundManager.Inst.Play(SoundManager.SoundType.BGM, "BGM", true);
            }

            _FSMC = FSMManager.Inst.NewFSMC();
            FSM fsm = _FSMC.Root_FSM;

            //标题界面
            fsm.AddState("Title",
                enter: (x) =>
                {

                    Started = false;
                    GameEnd = false;
                },
                exit: (x) =>
                {
                    StopAllCoroutines();
                    if (IsHostMode)
                    {
                        MyNetworkManager.Inst.ExpClientNum = IsTwoPlayers ? 2 : 1;
                        MyNetworkManager.Inst.StartHost();
                        //在双人游戏中，阻塞状态转移，以等待房间就绪
                        if (IsTwoPlayers && LocalPlayerNum == 1)
                            _FSMC.Block();
                    }
                    else
                        MyNetworkManager.Inst.StartClient();
                });

            //游戏中
            fsm = fsm.AddState("Game").SetAsFSM("Game", false);
            //载入场景
            fsm.AddState("SceneLoad",
                enter: (x) =>
                {
                    Player.P1 = null;
                    Player.P2 = null;
                    if (NetworkServer.active)
                        MyNetworkManager.Inst.ServerChangeScene("Main");
                },
                update: (x) =>
                {
                    if (!NetworkServer.active)
                        return;

                    if (IsTwoPlayers)
                    {
                        if (NetPlayer.P1 != null && NetPlayer.P2 != null)
                        {
                            NetPlayer.P1.Started();
                            NetPlayer.P1.PlayerControl = true;
                            NetPlayer.P2.PlayerControl = true;
                            Started = true;
                        }
                    }
                    else
                    {
                        if (NetPlayer.P1 != null)
                        {
                            NetPlayer.P1.Started();
                            NetPlayer.P1.PlayerControl = true;
                            Started = true;
                        }
                    }
                });
            //进入游戏
            fsm.AddState("InGame",
                enter: (x) =>
                {
                    if (MainCamera == null)
                        MainCamera = Camera.main;
                    float cam_width = MainCamera.orthographicSize * 2 * Screen.width / Screen.height;

                    Transform tr = GameObject.Find("LeftWall").transform;
                    Vector3 v = tr.localPosition;
                    v.x = -cam_width / 2;
                    tr.localPosition = v;

                    tr = GameObject.Find("RightWall").transform;
                    v = tr.localPosition;
                    v.x = cam_width / 2;
                    tr.localPosition = v;

                    //if (IsTwoPlayers)
                    //    Player.MaxDistance = cam_width - 2;

                    StartCoroutine(__GameStartSoundPlay());
                },
                exit: (x) =>
                {
                    if (NetworkServer.active)
                        MyNetworkManager.Inst.StopHost();
                    else
                        MyNetworkManager.Inst.StopClient();
                    SceneManager.LoadScene("Title");
                });
            fsm.SetBeginPath("SceneLoad", FSM.ALWAYS);
            fsm.AddTransferPath("SceneLoad", "InGame", (x) => Started);
            fsm.SetExitPath("InGame", FSM.MANUAL);

            fsm = fsm.Parent_State.Attached_FSM;
            fsm.SetBeginPath("Title", FSM.ALWAYS);
            //fsm.AddTransferPath("Title", "Title", FSM.MANUAL);
            fsm.AddTransferPath("Title", "Game", FSM.MANUAL);
            fsm.AddTransferPath("Game", "Title", FSM.ALWAYS);
        }

        protected override void OnAwake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            GlobalData.Inst = _GD;

            Bullet.LoadBulletData();
            Ammo.LoadPrefabs();
            FlyAmmoBoxSpawner.LoadPrefab();
            SoundManager.Inst.LoadSounds();

            //注册一个名为GameManager的邮箱
            MessageManager.Inst.Regist("GameManager");

            //当玩家模式选择完毕时，可以开启Host/Client
            MessageManager.Inst.CreateFilter("GameManager",
                (in GameMessage gm) => gm.Type == 0,
                (in GameMessage gm) =>
                {
                    IsHostMode = gm.Msg0.Bool;
                    IsTwoPlayers = gm.Msg1.Bool;
                    LocalPlayerNum = gm.Msg2.Int;
                    if (!IsHostMode)
                    {
                        MyNetworkManager.Inst.networkAddress = gm.Msg3.String;
                        (MyNetworkManager.Inst.transport as kcp2k.KcpTransport).Port = (ushort)gm.Msg4;
                        //Debug.Log(gm.Msg3.String);
                    }
                    //MyNetworkManager.Inst.transport.
                    _FSMC.StateTransfer("Game");
                });
            //当房间就绪时，Host开始游戏
            MessageManager.Inst.CreateFilter("GameManager",
                 (in GameMessage gm) => gm.Type == 1,
                 (in GameMessage gm) => _FSMC.CancelBlock());
            //Boss被击败
            MessageManager.Inst.CreateFilter("GameManager",
                (in GameMessage gm) => gm.Type == 2,
                (in GameMessage gm) =>
                {
                    if (NetworkServer.active)
                    {
                        Vector3 ep = GameObject.Find("ExitPos").transform.position;

                        NetPlayer.P1.PlayerControl = false;
                        Player.P1.GotoExitPos(ep);
                        if (IsTwoPlayers)
                        {
                            NetPlayer.P2.PlayerControl = false;
                            Player.P2.GotoExitPos(ep);
                        }
                        NetPlayer.P1.PlaySound(SoundManager.SoundType.BGM, "Success", false);
                    }

                    GameObject.Find("RightWall").SetActive(false);
                });
            //游戏结束，回到Title
            MessageManager.Inst.CreateFilter("GameManager",
                (in GameMessage gm) => gm.Type == 3,
                (in GameMessage gm) => _FSMC.StateExit());

            //当Netplayer被添加时（进入game scene以后），此时，Host为它们在对应位置生成Player
            MyNetworkManager.Inst.OnServerPlayerAdded += (x, y) =>
            {
                if (y == NetworkServer.localConnection)
                {
                    NetworkServer.Spawn(
                        Instantiate(
                            MyNetworkManager.Inst.spawnPrefabs.Find((x) => x.name == "Player"),
                            GameObject.Find("P1SpawnPos").transform.position,
                            Quaternion.identity));
                }
                else
                {
                    NetworkServer.Spawn(
                            Instantiate(
                                MyNetworkManager.Inst.spawnPrefabs.Find((x) => x.name == "Player"),
                                GameObject.Find("P2SpawnPos").transform.position,
                                Quaternion.identity));
                }

                //本地双人的情况下
                if (IsTwoPlayers && LocalPlayerNum == 2)
                {
                    NetPlayer player = Instantiate(MyNetworkManager.Inst.playerPrefab).GetComponent<NetPlayer>();
                    NetworkServer.Spawn(player.gameObject, NetworkServer.localConnection);
                    player.SetPID(2);
                    NetworkServer.Spawn(
                        Instantiate(
                            MyNetworkManager.Inst.spawnPrefabs.Find((x) => x.name == "Player"),
                            GameObject.Find("P2SpawnPos").transform.position,
                            Quaternion.identity));
                }
            };

            //初始化作弊码
            CheatCodeManager.AddCheatCode("30Lives",
                new CheatCodeKey[]
                {
                    CheatCodeKey.Up,CheatCodeKey.Up,
                    CheatCodeKey.Down,CheatCodeKey.Down,
                    CheatCodeKey.Left,CheatCodeKey.Right,
                    CheatCodeKey.Left,CheatCodeKey.Right,
                    CheatCodeKey.B,CheatCodeKey.A
                },
                (ch) =>
                {
                    if (NetworkClient.active)
                    {
                        ch.NPlayer.SetLifeCount(30);
                        ch.NPlayer.GameUIInfo($"Player{ch.PID}: 30Lives");
                    }
                });
            CheatCodeManager.AddCheatCode("Invincible",
                new CheatCodeKey[]
                {
                    CheatCodeKey.Up,CheatCodeKey.Up,
                    CheatCodeKey.Down,CheatCodeKey.Down,
                    CheatCodeKey.Left,CheatCodeKey.Right,
                    CheatCodeKey.Left,CheatCodeKey.Right,
                    CheatCodeKey.A,CheatCodeKey.A,CheatCodeKey.A
                },
                (ch) =>
                {
                    if (NetworkClient.active)
                    {
                        ch.NPlayer.SetInvincible(true);
                        ch.NPlayer.GameUIInfo($"Player{ch.PID}: Invincible");
                    }
                });

            //启动状态机
            _FSMC.Run();
        }

        private void Update()
        {
            if (MainCamera != null)
                GeometryUtility.CalculateFrustumPlanes(MainCamera, _FrustumPlanes);
        }

        private void FixedUpdate()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
                Application.Quit();
        }

        public bool IsInCameraView(Bounds b)
        {
            return GeometryUtility.TestPlanesAABB(_FrustumPlanes, b);
        }
    }
}
