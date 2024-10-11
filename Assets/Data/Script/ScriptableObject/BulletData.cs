using UnityEngine;

[CreateAssetMenu(fileName = "BulletData", menuName = "ScriptableObject/BulletData", order = 0)]
public class BulletData : ScriptableObject
{
    public string Name;

    public float Speed;

    public float CD;

    public bool IsPlayerBullet;

    public bool IsHasGravity;

    public GameObject Prefab;
}
