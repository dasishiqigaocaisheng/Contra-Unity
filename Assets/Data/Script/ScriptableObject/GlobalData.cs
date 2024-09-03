using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Modules.LogSystem;

[CreateAssetMenu(fileName = "GlobalData", menuName = "ScriptableObject/GlobalData", order = 0)]
public class GlobalData : ScriptableObject
{
    public static GlobalData Inst;

    public float PlayerJumpSpeed;

    public float EnemyJumpSpeed;

    public int MaxLiveBulletNum0;

    public int MaxLiveBulletNum1;

    public float BridgeBoomInterval;

    public int BatteryHP;

    public float FlyAmmoBoxSpeed;

    public float FlyAmmoBoxFluctuateAmpl;

    public float FlyAmmoBoxFluctuateSpeed;

    public int BossHP;

    public int BossGunHP;
}
