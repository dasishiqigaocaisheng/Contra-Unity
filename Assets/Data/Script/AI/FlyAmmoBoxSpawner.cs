using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Mirror;
using Contra.AI;

namespace Contra
{
    /// <summary>
    /// 只会在Server实例化
    /// </summary>
    public class FlyAmmoBoxSpawner : MonoBehaviour
    {
        public Gun.Type AmmoType { get; set; }

        private static GameObject _Prefab;

        public static void LoadPrefab()
        {
            _Prefab = Addressables.LoadAssetAsync<GameObject>("FlyAmmoBox").WaitForCompletion();
        }

        // Update is called once per frame
        void Update()
        {
            if (!GameManager.Inst.Started)
                return;

            if ((Player.P1.transform.position - transform.position).x > 0)
            {
                Vector3 v = GameManager.Inst.MainCamera.WorldToViewportPoint(transform.position);
                if (v.x < 0)
                {
                    FlyAmmoBox fab = Instantiate(_Prefab, transform.position + Vector3.left, Quaternion.identity).GetComponent<FlyAmmoBox>();
                    fab.AmmoType = AmmoType;
                    NetworkServer.Spawn(fab.gameObject);
                    Destroy(gameObject);
                }
            }
        }
    }
}
