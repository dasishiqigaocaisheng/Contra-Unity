using System.Linq;
using System.Net;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Mirror;
using Modules.MessageSystem;
using Contra.Network;

namespace Contra.UI
{
    public class TitleCanvas : MonoBehaviour
    {
        private List<GameObject> _Pages = new List<GameObject>();

        private bool _IsRoomReady;

        private int _ActivePageIndex = 1;
        /// <summary>
        /// 当前所处的页面索引
        /// </summary>
        public int ActivePageIndex
        {
            get => _ActivePageIndex;
            set
            {
                if (_ActivePageIndex == value) return;
                if (value >= 0 && value < _Pages.Count)
                {
                    _Pages[_ActivePageIndex].SetActive(false);
                    _Pages[value].SetActive(true);
                    _ActivePageIndex = value;
                }
            }
        }


        private void Start()
        {
            MessageManager.Inst.Regist("Title");
            //当Client链接建立时
            MessageManager.Inst.CreateFilter("Title",
                (in GameMessage gm) => gm.Sender.Name == "MyNetworkManager" && gm.Type == 0,
                (in GameMessage gm) =>
                {
                    if (ActivePageIndex == 3)
                    {
                        Debug.Log("room ready msg");
                        Text txt = _Pages[ActivePageIndex].transform.Find("Room/P2IP").GetComponent<Text>();
                        txt.text = (gm.Msg4 as NetworkConnectionToClient).address;
                        _IsRoomReady = true;
                    }
                });
            //已经发现服务器
            MessageManager.Inst.CreateFilter("Title",
                (in GameMessage gm) => gm.Sender.Name == "MyNetworkManager" && gm.Type == 1,
                (in GameMessage gm) =>
                {
                    SelectList sl = _Pages[2].transform.Find("SelectList").GetComponent<SelectList>();
                    sl.Clear();
                    sl.Title = $"   LAN ROOM {MyNetworkManager.Inst.ServerFound.Count}";
                    MyNetworkManager.Inst.ServerFound.ForEach(x => sl.Add(x.Host.ToUpper()));
                });

            UIInput.Inst.OnConfirm.AddListener(() =>
            {
                if (_IsRoomReady)
                {
                    GameMessage gm = new GameMessage { Type = 1 };
                    MessageManager.Inst.Send("Title", "GameManager", ref gm);
                }
                else if (ActivePageIndex == 4)
                {
                    InputField iptf = _Pages[ActivePageIndex].transform.Find("HostIP").GetComponent<InputField>();
                    if (IPAddress.TryParse(iptf.text, out _))
                    {
                        GameMessage gm = new GameMessage { Type = 0, Msg0 = false, Msg1 = true, Msg2 = 1, Msg3 = iptf.text };
                        MessageManager.Inst.Send("Title", "GameManager", ref gm);
                    }
                }
            });
            UIInput.Inst.OnEscape.AddListener(() =>
            {
                if (ActivePageIndex == 2)
                    MyNetworkManager.Inst.StopClient();
                else if (ActivePageIndex == 3)
                {
                    MyNetworkManager.Inst.StopHost();
                    ActivePageIndex = 1;
                    return;
                }
                else if (ActivePageIndex == 4)
                {
                    MyNetworkManager.Inst.Discovery.StopDiscovery();
                    ActivePageIndex = 1;
                    return;
                }
                ActivePageIndex--;
            });

            for (int i = 0; i < transform.childCount; i++)
                _Pages.Add(transform.GetChild(i).gameObject);

            _Pages[0].GetComponent<RectTransform>().anchoredPosition = Vector2.right * GetComponent<RectTransform>().rect.width;
            ActivePageIndex = 0;
            _Pages[0].GetComponent<RectTransform>().DOAnchorPosX(0, 3).SetEase(Ease.Linear).onComplete = () =>
            {
                transform.Find("Page0/Pic").gameObject.SetActive(true);
                transform.Find("Page0/SelectList/Cursor").gameObject.SetActive(true);
                UIInput.Inst.Enable = true;
                SoundManager.Inst.Play(SoundManager.SoundType.BGM, "Title", false);
            };
        }

        private void OnDestroy()
        {
            if (MessageManager.Inst != null)
                MessageManager.Inst.UnRegist("Title");
        }

        public void _OnSelectListSelected(int idx)
        {
            switch ((_ActivePageIndex, idx))
            {
                case (0, 0):
                    {
                        //Msg0：是否为Host
                        //Msg1：是否为多人游戏
                        //Msg2：本地玩家个数
                        GameMessage gm = new GameMessage { Type = 0, Msg0 = true, Msg1 = false, Msg2 = 1 };
                        MessageManager.Inst.Send("Title", "GameManager", ref gm);
                    }
                    break;
                case (0, 1):
                    ActivePageIndex = 1;
                    break;
                //关闭游戏
                case (0, 2):
                    Application.Quit();
                    break;
                //双人本地
                case (1, 0):
                    {
                        GameMessage gm = new GameMessage { Type = 0, Msg0 = true, Msg1 = true, Msg2 = 2 };
                        MessageManager.Inst.Send("Title", "GameManager", ref gm);
                    }
                    break;
                //双人Host
                case (1, 1):
                    {
                        GameMessage gm = new GameMessage { Type = 0, Msg0 = true, Msg1 = true, Msg2 = 1 };
                        MessageManager.Inst.Send("Title", "GameManager", ref gm);
                        ActivePageIndex = 3;
                    }
                    break;
                //双人Client LAN
                case (1, 2):
                    {
                        ActivePageIndex = 2;
                        MyNetworkManager.Inst.Discovery.StartDiscovery();
                    }
                    break;
                //双人Client Custom
                case (1, 3):
                    ActivePageIndex = 4;
                    break;
                //Client选中Host
                case (2, _):
                    {
                        GameMessage gm = new GameMessage { Type = 0, Msg0 = false, Msg1 = true, Msg2 = 1, Msg3 = MyNetworkManager.Inst.ServerFound[idx].Host };
                        MessageManager.Inst.Send("Title", "GameManager", ref gm);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}