using UnityEngine;

namespace Contra.Bullets
{
    public class FireBall : Bullet
    {
        private float _Angle = 90;

        private new FireballBulletData Data => base.Data as FireballBulletData;

        protected override void Start()
        {
            base.Start();
            transform.Translate(transform.up * Data.RotateRadius);
        }

        protected void FixedUpdate()
        {
            Vector2 l_vel = new Vector2(Mathf.Sin(_Angle * Mathf.PI / 180f), -Mathf.Cos(_Angle * Mathf.PI / 180f)) * Data.RotateRadius * Data.RotateSpeed * Mathf.PI / 180f;
            _Rigid.velocity = l_vel + (Vector2)_MoveDir * Data.Speed;

            _Angle -= Data.RotateSpeed * Time.fixedDeltaTime;
            if (_Angle < 0f)
                _Angle += 360f;
        }
    }
}
