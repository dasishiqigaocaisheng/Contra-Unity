using UnityEngine;
using Mirror;

namespace Contra.AI
{
    public class FlyAmmoBox : MonoBehaviour
    {
        public Gun.Type AmmoType { get; set; }

        private Rigidbody2D _Rgd2D;

        private float _StartTime;

        private Vector2 _StartPos;

        private void Awake()
        {
            _Rgd2D = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            _StartTime = Time.fixedTime;
            _StartPos = transform.position;

            _Rgd2D.simulated = NetworkServer.active;
        }

        private void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                //_Rgd2D.velocity = Vector2.right * GlobalData.Inst.FlyAmmoBoxSpeed;
                //_Rgd2D.velocity += GlobalData.Inst.FlyAmmoBoxFluctuateAmpl * Mathf.Cos(2 * Mathf.PI * GlobalData.Inst.FlyAmmoBoxFluctuateSpeed * (Time.fixedTime - _StartTime)) * Vector2.up;

                //这里使用位置模拟而不是上面的速度模拟
                //速度模拟会出现累积误差，原因未知
                _StartPos += GlobalData.Inst.FlyAmmoBoxSpeed * Time.deltaTime * Vector2.right;
                _Rgd2D.MovePosition(_StartPos +
                    GlobalData.Inst.FlyAmmoBoxFluctuateAmpl * Mathf.Sin(2 * Mathf.PI * GlobalData.Inst.FlyAmmoBoxFluctuateSpeed * (Time.fixedTime - _StartTime)) * Vector2.up);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("PlayerBullet"))
            {
                EffectManager.Inst.PlayEffect("Boom0", transform.position);

                Rigidbody2D rdg2d = Instantiate(Ammo.Prefabs[AmmoType], transform.position, Quaternion.identity).GetComponent<Rigidbody2D>();
                rdg2d.velocity = new Vector2(3, 5);
                rdg2d.gameObject.name = AmmoType.ToString();

                Destroy(gameObject);
            }
        }

        private void OnBecameInvisible()
        {
            if (NetworkServer.active && Player.P1 != null)
            {
                if (transform.position.x > Player.P1.transform.position.x)
                    Destroy(gameObject);
            }
        }
    }
}
