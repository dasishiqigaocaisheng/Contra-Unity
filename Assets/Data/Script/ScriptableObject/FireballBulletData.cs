using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FireballBulletData", menuName = "ScriptableObject/FireballBulletData", order = 1)]
public class FireballBulletData : BulletData
{
    public float RotateSpeed;

    public float RotateRadius;
}
