using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Modules.LogSystem;
using Modules.Utility.Singleton;

namespace Modules.MessageSystem
{
    public class MessageManager : MonoSingleton<MessageManager>, IModule
    {
        [SerializeField]
        private int _WarnUserCapacity = 1024;

        [SerializeField]
        private int _DefaultMsgBoxWarnThreshold = 16;

        private Dictionary<int, MessageUser> _UserInfos = new Dictionary<int, MessageUser>();

        private Dictionary<string, int> _Name2UID = new Dictionary<string, int>();

        private List<MessageFilter> _FiltersToProcess = new List<MessageFilter>();

        private List<MessageFilter> _FiltersToProcessPending = new List<MessageFilter>();

        private bool _IsProcessing;

        protected override void OnAwake()
        {
            DontDestroyOnLoad(gameObject);
            StartCoroutine(_FiltersProcess());
        }

        public void Prepare()
        {
            LogManager.Info("MessageSystem.MessageManager", "MessageSystem已经初始化");
        }

        public int Regist(string name = null, int warn_th = 0)
        {
            if (name != null && _Name2UID.ContainsKey(name))
            {
                LogManager.Error("MessageManager.MessageManager", "注册名已经存在", args: ("注册名", name));
                return -1;
            }

            int uid = _GenerateUID();
            int cap = warn_th <= 0 ? _DefaultMsgBoxWarnThreshold : warn_th;


            _UserInfos.Add(uid, new MessageUser
            {
                Info = new MessageUserInfo { Name = name, UID = uid },
                MsgBox = new MessageBox(cap)
            });

            if (name != null)
                _Name2UID.Add(name, uid);

            if (_UserInfos.Count > _WarnUserCapacity)
                LogManager.Warn("MessageSystem.MessageManager", $"当前注册用户数量（{_UserInfos.Count}）已经超过{_WarnUserCapacity}");

            return uid;
        }

        public bool RegistWithCertainUID(int uid, string name = null, int warn_th = 0)
        {
            if (uid < 0)
            {
                LogManager.Error("MessageManager.MessageManager", "uid小于零", args: ("uid", uid));
                return false;
            }

            if (name != null && _Name2UID.ContainsKey(name))
            {
                LogManager.Error("MessageManager.MessageManager", "注册名已经存在", args: ("注册名", name));
                return false;
            }

            if (!_UserInfos.ContainsKey(uid))
            {
                int cap = warn_th <= 0 ? _DefaultMsgBoxWarnThreshold : warn_th;

                _UserInfos.Add(uid, new MessageUser
                {
                    Info = new MessageUserInfo { Name = name, UID = uid },
                    MsgBox = new MessageBox(cap)
                });

                if (name != null)
                    _Name2UID.Add(name, uid);

                if (_UserInfos.Count > _WarnUserCapacity)
                    LogManager.Warn("MessageSystem.MessageManager", $"当前注册用户数量（{_UserInfos.Count}）已经超过{_WarnUserCapacity}");

                return true;
            }
            else
                return false;
        }

        public void UnRegist(int uid)
        {
            if (!_UserInfos.ContainsKey(uid))
            {
                LogManager.Error("MessageManager.UnRegist", "要注销的UID不存在", args: ("UID", uid));
                return;
            }

            //移除Name->UID的记录
            if (_UserInfos.TryGetValue(uid, out MessageUser user))
            {
                if (_Name2UID.ContainsKey(user.Info.Name))
                    _Name2UID.Remove(user.Info.Name);
            }

            //移除注册信息
            _UserInfos.Remove(uid);
        }

        public void UnRegist(string name)
        {
            LogManager.Assert(_Name2UID.ContainsKey(name), "MessageManager.UnRegist", "要注销的用户名不存在", args: ("name", name));

            UnRegist(_Name2UID[name]);
        }

        public int CreateFilter(int uid, string fname, FilterMatchFunc match, MessageReceivedCallback cb)
        {
            if (_UserInfos.TryGetValue(uid, out MessageUser user))
                return user.MsgBox.CreateFilter(fname, match, cb);
            else
                return -1;
        }

        public int CreateFilter(int uid, FilterMatchFunc match, MessageReceivedCallback cb)
        {
            if (_UserInfos.TryGetValue(uid, out MessageUser user))
                return user.MsgBox.CreateFilter(match, cb);
            else
                return -1;
        }

        public int CreateFilter(string uname, string fname, FilterMatchFunc match, MessageReceivedCallback cb)
        {
            LogManager.Assert(_Name2UID.TryGetValue(uname, out int uid), "MessageSystem.MessageManager", $"用户名{uname}不存在");

            return CreateFilter(uid, fname, match, cb);
        }

        public int CreateFilter(string uname, FilterMatchFunc match, MessageReceivedCallback cb)
        {
            LogManager.Assert(_Name2UID.TryGetValue(uname, out int uid), "MessageSystem.MessageManager", $"用户名{uname}不存在");

            return CreateFilter(uid, match, cb);
        }

        public void Send(int from, int to, ref GameMessage msg)
        {
            if (!_UserInfos.TryGetValue(from, out MessageUser from_usr))
            {
                LogManager.Error("MessageManager.MessageManager", "UID不存在", args: ("UID", from));
                return;
            }

            if (!_UserInfos.TryGetValue(to, out MessageUser to_usr))
            {
                //Error
                Debug.Log("Error");
                return;
            }

            msg.Sender = from_usr.Info;
            msg.Receiver = to_usr.Info;

            MessageFilter mf = to_usr.MsgBox.Send(in msg);
            if (!mf.UpdateThisFrame && mf.Callback != null)
            {
                if (!_IsProcessing)
                    _FiltersToProcess.Add(mf);
                else
                    _FiltersToProcessPending.Add(mf);
                mf.UpdateThisFrame = true;
            }
        }

        public void Send(string from, string to, ref GameMessage msg)
        {
            LogManager.Assert(_Name2UID.TryGetValue(from, out int from_id), "MessageSystem.MessageManager", $"发送方（{from}）不存在");
            LogManager.Assert(_Name2UID.TryGetValue(to, out int to_id), "MessageSystem.MessageManager", $"接收方（{to}）不存在");

            Send(from_id, to_id, ref msg);
        }

        public void SendNoCache(int from, int to, ref GameMessage msg)
        {
            LogManager.Assert(_UserInfos.TryGetValue(from, out MessageUser from_usr), "MessageSystem.MessageManager", $"发送方UID（{from}）不存在");
            LogManager.Assert(_UserInfos.TryGetValue(to, out MessageUser to_usr), "MessageSystem.MessageManager", $"接收方UID（{to}）不存在");

            msg.Sender = from_usr.Info;
            msg.Receiver = to_usr.Info;

            to_usr.MsgBox.SendNoCache(in msg);
        }

        public void SendNoCache(int to, ref GameMessage msg)
        {
            LogManager.Assert(_UserInfos.TryGetValue(to, out MessageUser to_usr), "MessageSystem.MessageManager", $"接收方UID（{to}）不存在");

            msg.Sender.UID = -1;
            msg.Receiver = to_usr.Info;

            to_usr.MsgBox.SendNoCache(in msg);
        }

        public void SendNoCache(string to, ref GameMessage msg)
        {
            LogManager.Assert(_Name2UID.TryGetValue(to, out int to_id), "MessageSystem.MessageManager", $"接收方（{to}）不存在");

            SendNoCache(to_id, ref msg);
        }

        public void Send(int to, ref GameMessage msg)
        {
            if (!_UserInfos.TryGetValue(to, out MessageUser to_usr))
            {
                //Error
                Debug.Log("Error");
                return;
            }

            msg.Sender.UID = -1;
            msg.Receiver = to_usr.Info;

            MessageFilter mf = to_usr.MsgBox.Send(in msg);
            if (!mf.UpdateThisFrame && mf.Callback != null)
            {
                if (!_IsProcessing)
                    _FiltersToProcess.Add(mf);
                else
                    _FiltersToProcessPending.Add(mf);
                mf.UpdateThisFrame = true;
            }
        }

        public void Send(string to, ref GameMessage msg)
        {
            LogManager.Assert(_Name2UID.TryGetValue(to, out int to_id), "MessageSystem.MessageManager", $"接收方（{to}）不存在");

            Send(to_id, ref msg);
        }

        public bool TryNext(int uid, out GameMessage msg, string filter)
        {
            if (!_UserInfos.ContainsKey(uid))
            {
                //Error
                Debug.Log("Error");
                msg = new GameMessage();
                return false;
            }

            return _UserInfos[uid].MsgBox.TryGet(out msg, filter);
        }

        public int GetUserIDWithName(string name)
        {
            if (name == null)
            {
                //Error
                Debug.Log("Error");
                return -1;
            }

            if (_Name2UID.TryGetValue(name, out int id))
                return id;
            else
                return -1;
        }

        public string GetUserNameWithID(int uid)
        {
            if (uid < 0)
            {
                //Error
                Debug.Log("Error");
                return null;
            }

            if (_UserInfos.TryGetValue(uid, out MessageUser mu))
                return mu.Info.Name;
            else
                return null;
        }

        private int _GenerateUID()
        {
            int uid;
            do
            {
                uid = Random.Range(0, int.MaxValue);
            }
            while (_UserInfos.ContainsKey(uid));

            return uid;
        }

        //TODO:当执行_FiltersProcess时执行Send会修改迭代器
        private IEnumerator _FiltersProcess()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();

                _FiltersToProcess.AddRange(_FiltersToProcessPending);
                _FiltersToProcessPending.Clear();
                if (_FiltersToProcess.Count != 0)
                {
                    _IsProcessing = true;
                    foreach (MessageFilter mf in _FiltersToProcess)
                    {
                        while (mf.MsgQueue.Count != 0)
                        {
                            GameMessage gm = mf.MsgQueue.Dequeue();
                            mf.Callback(in gm);
                        }
                        mf.UpdateThisFrame = false;
                    }
                    _FiltersToProcess.Clear();
                    _IsProcessing = false;
                }
            }
        }
    }
}
