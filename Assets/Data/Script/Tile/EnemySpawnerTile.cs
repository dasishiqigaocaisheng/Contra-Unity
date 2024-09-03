using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[CreateAssetMenu(fileName = "EnemySpawnerTile", menuName = "2D/Tiles/EnemySpawnerTile")]
public class EnemySpawnerTile : Tile
{
    public GameObject SpawnerPrefab;

    public override bool StartUp(Vector3Int location, ITilemap tilemap, GameObject go)
    {
        if (Application.isPlaying)
            Instantiate(SpawnerPrefab, location, Quaternion.identity);

        return true;
    }
}
