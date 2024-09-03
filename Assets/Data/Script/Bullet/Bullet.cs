using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Mirror;
using DG.Tweening;
using Contra.Network;

namespace Contra.Bullets
{
    public class Bullet : MonoBehaviour
    {
        protected Rigidbody2D _Rigid;

        public BulletData Data;

        protected Vector3 _MoveDir;

        private SpriteRenderer _Sprite;

        public static int PlayerBulletNum { get; private set; }

        private static Dictionary<string, BulletData> _BulletDataLUT = new Dictionary<string, BulletData>();

        public static void LoadBulletData()
        {
            Addressables.LoadAssetsAsync<BulletData>("BulletData", (x) => _BulletDataLUT.Add(x.Name, x)).WaitForCompletion();
        }

        public static Bullet Spawn(string name, Vector3 pos, Vector3 dir)
        {
            BulletData bd = _BulletDataLUT[name];
            Quaternion q = Quaternion.FromToRotation(Vector3.right, dir);
            Bullet b = Instantiate(bd.Prefab, pos, q).GetComponent<Bullet>();
            NetworkServer.Spawn(b.gameObject);

            b._MoveDir = dir.normalized;

            return b;
        }

        protected virtual void Awake()
        {
            _Rigid = GetComponent<Rigidbody2D>();
            _Rigid.gravityScale = Data.IsHasGravity ? 1 : 0;
            _Sprite = GetComponent<SpriteRenderer>();

            if (!NetworkServer.active)
                _Rigid.simulated = false;
            else if (Data.IsPlayerBullet)
                PlayerBulletNum++;

        }

        protected virtual void Start()
        {
            _Rigid.velocity = Data.Speed * _MoveDir;
        }

        private void FixedUpdate()
        {
            if (!NetworkServer.active)
                return;

            if (!GameManager.Inst.IsInCameraView(_Sprite.bounds))
                _DestroySelf();
        }

        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            if (Data.IsPlayerBullet)
            {
                if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                {
                    _DestroySelf();
                    EffectManager.Inst.PlayEffect("BulletBoom", transform.position);
                }
            }
            else
            {
                if (collision.gameObject.layer == LayerMask.NameToLayer("Player0"))
                    _DestroySelf();
            }
        }

        private void _DestroySelf()
        {
            for (int i = 0; i < transform.childCount; i++)
                NetworkServer.Destroy(transform.GetChild(i).gameObject);
            NetworkServer.Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (NetworkServer.active && Data.IsPlayerBullet)
                PlayerBulletNum--;
        }
    }
}
