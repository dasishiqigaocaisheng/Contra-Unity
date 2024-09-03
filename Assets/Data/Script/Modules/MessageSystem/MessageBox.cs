using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Modules.LogSystem;

namespace Modules.MessageSystem
{
    internal class MessageBox
    {
        public int WarnThreshold { get; set; }

        private int _RecentMsgNum;

        public MessageFilter DefaultFilter => _Filters[0];

        private List<MessageFilter> _Filters;

        public MessageBox(int warn_th)
        {
            WarnThreshold = warn_th;
            _Filters = new List<MessageFilter>
            {
                new MessageFilter()
            };
        }

        public MessageFilter Send(in GameMessage msg)
        {
            for (int i = 1; i < _Filters.Count; i++)
            {
                if (_Filters[i].MatchFunc(in msg))
                {
                    _Filters[i].MsgQueue.Enqueue(msg);

                    if (++_RecentMsgNum == WarnThreshold)
                    {
                        //Warn
                    }

                    return _Filters[i];
                }
            }

            _Filters[0].MsgQueue.Enqueue(msg);
            _Filters[0].UpdateThisFrame = true;

            if (++_RecentMsgNum == WarnThreshold)
            {
                //Warn
            }

            return _Filters[0];
        }

        public void SendNoCache(in GameMessage msg)
        {
            for (int i = 1; i < _Filters.Count; i++)
            {
                if (_Filters[i].MatchFunc(in msg))
                {
                    _Filters[i].Callback(in msg);
                    return;
                }
            }

            if (_Filters[0].Callback != null)
                _Filters[0].Callback(in msg);
            else
                LogManager.Error("MessageSystem.MessageBox", "没有合适的过滤器");
        }

        public bool TryGet(out GameMessage msg, string filter_name)
        {
            MessageFilter mf = filter_name == null ? _Filters[0] : _Filters.FirstOrDefault(x => x.Name == filter_name);
            if (mf != null)
            {
                if (mf.MsgQueue.TryDequeue(out msg))
                {
                    _RecentMsgNum--;
                    return true;
                }
                return false;
            }
            else
            {
                //Error
                Debug.LogError("error");
                msg = new GameMessage();
                return false;
            }
        }

        public bool TryGet(out GameMessage msg, int filter_idx)
        {
            if (filter_idx < _Filters.Count && filter_idx >= 0)
            {
                if (_Filters[filter_idx].MsgQueue.TryDequeue(out msg))
                {
                    _RecentMsgNum--;
                    return true;
                }
                return false;
            }
            else
            {
                //Error
                msg = new GameMessage();
                return false;
            }
        }

        public bool TryGetFromDefault(out GameMessage msg)
        {
            return TryGet(out msg, 0);
        }

        public int CreateFilter(string name, FilterMatchFunc match, MessageReceivedCallback cb)
        {
            if (name == null)
                return CreateFilter(match, cb);

            if (!_Filters.Exists(x => x.Name == name))
            {
                _Filters.Add(new MessageFilter { Name = name, MatchFunc = match, Callback = cb });
                return _Filters.Count - 1;
            }
            else
            {
                //Error
                return -1;
            }
        }

        public int CreateFilter(FilterMatchFunc match, MessageReceivedCallback cb)
        {
            _Filters.Add(new MessageFilter { MatchFunc = match, Callback = cb });
            return _Filters.Count - 1;
        }
    }
}
