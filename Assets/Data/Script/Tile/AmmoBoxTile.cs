using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Contra.AI;

namespace Contra.Tile
{
    [CreateAssetMenu(fileName = "AmmoBoxTile", menuName = "2D/Tiles/AmmoBoxTile")]
    public class AmmoBoxTile : MapObjectTile
    {
        [SerializeField]
        private Gun.Type _GunType;

        [SerializeField]
        private bool _IsFlyAmmoBox;

        public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
        {
            base.StartUp(position, tilemap, go);
            if (_MapObjectInstance != null)
            {
                if (!_IsFlyAmmoBox)
                    _MapObjectInstance.GetComponent<AmmoBox>().GunType = _GunType;
                else
                    _MapObjectInstance.GetComponent<FlyAmmoBoxSpawner>().AmmoType = _GunType;
            }

            return true;
        }
    }
}
