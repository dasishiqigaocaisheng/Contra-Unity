using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[Obsolete]
[CreateAssetMenu(fileName = "BridgeTile", menuName = "2D/Tiles/BridgeTile")]
public class BridgeTile : Tile
{
    public int Length;

    public GameObject Head;

    public GameObject Body;

    public GameObject Tail;


    public override bool StartUp(Vector3Int location, ITilemap tilemap, GameObject go)
    {
        if (Application.isPlaying)
        {
            Transform bridge = new GameObject("Brigde").transform;
            Transform head = Instantiate(Head, bridge).transform;
            head.localPosition = new Vector3Int(4, 0);
            for (int i = 0; i < Length - 2; i++)
            {
                Transform body = Instantiate(Body, bridge).transform;
                body.localPosition = new Vector3Int((i + 2) * 4, 0);
            }
            Transform tail = Instantiate(Tail, bridge).transform;
            tail.localPosition = new Vector3Int((Length - 1), 0);
        }

        return true;
    }
}
