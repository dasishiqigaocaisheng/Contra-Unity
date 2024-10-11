using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;

[CreateAssetMenu(fileName = "MapObjectTile", menuName = "2D/Tiles/MapObjectTile")]
public class MapObjectTile : Tile
{
    [SerializeField]
    protected Vector3 _Offset;

    [SerializeField]
    protected GameObject _MapObjectPrefab;

    protected GameObject _MapObjectInstance;


    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);
        //当Tile被擦除时不要同时销毁物体|只在运行时生成
        tileData.flags |= TileFlags.KeepGameObjectRuntimeOnly | TileFlags.InstantiateGameObjectRuntimeOnly;
    }

    public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
    {
        if (Application.isPlaying && NetworkServer.active)
        {
            Tilemap tm = tilemap.GetComponent<Tilemap>();
            _MapObjectInstance = Instantiate(_MapObjectPrefab, tm.CellToWorld(position) + _Offset, Quaternion.identity);
            _MapObjectInstance.transform.SetParent(null);

            if (_MapObjectInstance.TryGetComponent<NetworkIdentity>(out _))
                NetworkServer.Spawn(_MapObjectInstance);
        }

        return true;
    }
}
