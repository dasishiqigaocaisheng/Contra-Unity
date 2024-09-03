using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Mirror;
using Modules.Utility;


namespace Contra
{
    public enum SpawnerType
    {
        OnRight,
        OnLeft,
        Once
    }

    public class EnemySpawner : MonoBehaviour
    {
        public SpawnerType Type;

        public float YOffset;

        public GameObject EnemyPrefab;

        private Tween _Tween;

        private void Start()
        {
            var tm = Map.Inst["Enemy"];
            tm.SetTile(tm.WorldToCell(transform.position), null);
            if (!NetworkServer.active)
                Destroy(gameObject);
        }

        private void Update()
        {
            bool __IsVisible()
            {
                Vector3 v = Camera.main.WorldToViewportPoint(transform.position);
                return v.x >= 0 && v.x <= 1;
            }

            if (!GameManager.Inst.Started || !NetworkServer.active)
                return;

            float dist = transform.position.x - Player.ClosestAliveOne(transform.position).transform.position.x;
            if (!__IsVisible() && dist.Abs() < 50)
            {
                if (Type == SpawnerType.OnRight && dist > 22)
                {
                    if (_Tween == null)
                    {
                        NetworkServer.Spawn(Instantiate(EnemyPrefab, transform.position + Vector3.up * YOffset, Quaternion.identity));
                        _Tween = DOVirtual.DelayedCall(Random.value * 4 + 3, () => _Tween = null);
                    }
                }
                else if (Type == SpawnerType.OnLeft && dist < -22)
                {
                    if (_Tween == null)
                    {
                        NetworkServer.Spawn(Instantiate(EnemyPrefab, transform.position + Vector3.up * YOffset, Quaternion.identity));
                        _Tween = DOVirtual.DelayedCall(Random.value * 4 + 3, () => _Tween = null);
                    }
                }
                else if (Type == SpawnerType.Once)
                {
                    NetworkServer.Spawn(Instantiate(EnemyPrefab, transform.position + Vector3.up * YOffset, Quaternion.identity));
                    Destroy(gameObject);
                }
            }
        }
    }
}
