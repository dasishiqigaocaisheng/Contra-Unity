using System.Collections;
using UnityEngine;
using Mirror;
using Contra.Network;

namespace Contra.AI
{
    public class AmmoBox : MonoBehaviour
    {
        private Animator _Anim;

        private BoxCollider2D _Cldr2D;

        public Gun.Type GunType { get; set; }

        private void Awake()
        {
            _Anim = transform.Find("Sprite").GetComponent<Animator>();
            _Cldr2D = GetComponent<BoxCollider2D>();
        }

        // Start is called before the first frame update
        void Start()
        {
            if (NetworkServer.active)
                StartCoroutine(_OpenAndClose());
        }

        private IEnumerator _OpenAndClose()
        {
            while (true)
            {
                yield return new WaitForSeconds(2);
                _Anim.SetBool("IsOpen", true);
                _Cldr2D.enabled = true;

                yield return new WaitForSeconds(2);
                _Anim.SetBool("IsOpen", false);
                _Cldr2D.enabled = false;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (NetworkServer.active && collision.gameObject.layer == LayerMask.NameToLayer("PlayerBullet"))
            {
                EffectManager.Inst.PlayEffect("Boom0", transform.position);
                NetPlayer.P1.PlaySound(SoundManager.SoundType.Effect, "Effect0", false);

                Rigidbody2D rdg2d = Instantiate(Ammo.Prefabs[GunType], transform.position, Quaternion.identity).GetComponent<Rigidbody2D>();
                NetworkServer.Spawn(rdg2d.gameObject);
                rdg2d.velocity = new Vector2(3, 15);
                rdg2d.gameObject.name = GunType.ToString();

                NetworkServer.Destroy(gameObject);
            }
        }
    }
}
