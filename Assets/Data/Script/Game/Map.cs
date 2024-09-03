using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Modules.MessageSystem;
using Modules.Utility.Singleton;
using DG.Tweening;

namespace Contra
{
    public class Map : MonoSingleton<Map>
    {
        public Dictionary<string, Tilemap> _Tilemaps { get; private set; } = new Dictionary<string, Tilemap>();

        public Tilemap this[string name] => _Tilemaps[name];

        public PlatformEffector2D Platform;

        public TilemapCollider2D BankCollider;


        protected override void OnAwake()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).TryGetComponent(out Tilemap tm))
                    _Tilemaps.Add(tm.name, tm);
            }
        }

        private void Start()
        {
            _Tilemaps["MapObject"].gameObject.SetActive(false);

            MessageManager.Inst.Regist("Map");
            //Type==0，Gound层允许玩家掉落
            //Msg0==1，P1；Msg0==2，P2
            MessageManager.Inst.CreateFilter("Map", null,
                (in GameMessage x) => x.Type == 0,
                (in GameMessage x) =>
                {
                    int layer = 1 << LayerMask.NameToLayer(x.Msg0.Int == 1 ? "Player0Foot" : "Player1Foot");
                    Platform.colliderMask &= ~layer;
                    DOVirtual.DelayedCall(0.3f, () => Platform.colliderMask |= layer);
                });
        }

        private void OnDestroy()
        {
            if (MessageManager.Inst != null)
                MessageManager.Inst.UnRegist("Map");
        }
    }
}
