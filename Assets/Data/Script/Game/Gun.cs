using System;
using UnityEngine;
using Contra.Bullets;
using Contra.Network;

namespace Contra
{
    [Serializable]
    public class Gun
    {
        public enum Type
        {
            Normal,
            Machine,
            Fireball,
            Laser,
            Shotgun,
            Rifle
        }

        private Type _GunType;
        public Type GunType
        {
            get => _GunType;
            set
            {
                _GunType = value;
                _TimeReg = 0;
                _IsInCD = false;
            }
        }

        private bool _IsInCD;

        private float _TimeReg;

        private float _CD;

        public bool Fire(Vector2 pos, Vector2 dir)
        {
            if (Time.time - _TimeReg > _CD)
                _IsInCD = false;

            if (!_IsInCD)
            {
                switch (GunType)
                {
                    case Type.Normal:
                        if (Bullet.PlayerBulletNum < GlobalData.Inst.MaxLiveBulletNum0)
                        {
                            _CD = Bullet.Spawn("Normal", pos, dir).Data.CD;
                            NetPlayer.P1.PlaySound(SoundManager.SoundType.Shoot, "ShootN", false);
                        }
                        break;
                    case Type.Machine:
                        if (Bullet.PlayerBulletNum < GlobalData.Inst.MaxLiveBulletNum0)
                        {
                            _CD = Bullet.Spawn("Machine", pos, dir).Data.CD;
                            NetPlayer.P1.PlaySound(SoundManager.SoundType.Shoot, "ShootM", false);
                        }
                        break;
                    case Type.Laser:
                        if (Bullet.PlayerBulletNum < GlobalData.Inst.MaxLiveBulletNum0)
                        {
                            _CD = Bullet.Spawn("Laser", pos, dir).Data.CD;
                            NetPlayer.P1.PlaySound(SoundManager.SoundType.Shoot, "ShootL", false);
                        }
                        break;
                    case Type.Fireball:
                        if (Bullet.PlayerBulletNum < GlobalData.Inst.MaxLiveBulletNum0)
                        {
                            _CD = Bullet.Spawn("Fireball", pos, dir).Data.CD;
                            NetPlayer.P1.PlaySound(SoundManager.SoundType.Shoot, "ShootF", false);
                        }
                        break;
                    case Type.Shotgun:
                        {
                            if (Bullet.PlayerBulletNum >= GlobalData.Inst.MaxLiveBulletNum1)
                                break;
                            _CD = Bullet.Spawn("Shotgun", pos, dir).Data.CD;
                            NetPlayer.P1.PlaySound(SoundManager.SoundType.Shoot, "ShootS", false);
                            if (Bullet.PlayerBulletNum >= GlobalData.Inst.MaxLiveBulletNum1)
                                break;
                            Quaternion q = Quaternion.Euler(0, 0, 7.5f);
                            Bullet.Spawn("Shotgun", pos, q * dir);
                            if (Bullet.PlayerBulletNum >= GlobalData.Inst.MaxLiveBulletNum1)
                                break;
                            Bullet.Spawn("Shotgun", pos, Quaternion.Inverse(q) * dir);
                            if (Bullet.PlayerBulletNum >= GlobalData.Inst.MaxLiveBulletNum1)
                                break;
                            q = Quaternion.Euler(0, 0, 15);
                            Bullet.Spawn("Shotgun", pos, q * dir);
                            if (Bullet.PlayerBulletNum >= GlobalData.Inst.MaxLiveBulletNum1)
                                break;
                            Bullet.Spawn("Shotgun", pos, Quaternion.Inverse(q) * dir);
                            break;
                        }
                    case Type.Rifle:
                        if (Bullet.PlayerBulletNum < GlobalData.Inst.MaxLiveBulletNum0)
                        {
                            _CD = Bullet.Spawn("Rifle", pos, dir).Data.CD;
                            NetPlayer.P1.PlaySound(SoundManager.SoundType.Shoot, "ShootN", false);
                        }
                        break;
                }
                _IsInCD = true;
                _TimeReg = Time.time;
                return true;
            }

            return false;
        }
    }
}
