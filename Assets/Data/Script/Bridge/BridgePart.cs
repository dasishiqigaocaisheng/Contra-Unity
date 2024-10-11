using System.Linq;
using System.Collections;
using UnityEngine;
using Mirror;
using Contra.Network;

namespace Contra.Bridge
{
    public enum BridgePartDef
    {
        Head,
        Body,
        Tail
    }

    public class BridgePart : NetworkBehaviour
    {
        public BridgePartDef Type;

        private BridgePart _Next;

        private bool _IsDestroying;

        [SerializeField]
        private ParticleSystem _Boom0;

        [SerializeField]
        private ParticleSystem _Boom1;

        [SerializeField]
        private ParticleSystem _Boom2;

        [SerializeField]
        private ParticleSystem _Boom3;

        private void Start()
        {
            if (Type == BridgePartDef.Head || Type == BridgePartDef.Body)
            {
                GameObject[] gos = GameObject.FindGameObjectsWithTag("Bridge").ToArray();
                //每个部分只寻找自己的下一个部分
                _Next = gos.First(x => x.transform.position.x > transform.position.x && (x.transform.position - transform.position).x < 5).GetComponent<BridgePart>();
            }
        }

        private void Update()
        {
            if (!NetworkServer.active || !GameManager.Inst.Started)
                return;

            //只有Head才去判断是否自毁
            if (!_IsDestroying && Type == BridgePartDef.Head)
            {
                if (transform.position.x - Player.ClosestAliveOne(transform.position).transform.position.x <= 0.1f)
                    Boom();
            }
        }

        [ClientRpc]
        public void Boom()
        {
            StartCoroutine(_BridgeDestroy());
        }

        private IEnumerator _BridgeDestroy()
        {
            _IsDestroying = true;

            //yield return new WaitForSeconds(0.4f);

            SoundManager.Inst.Play(SoundManager.SoundType.Effect, "Effect3", false);

            _Boom0.transform.SetParent(null);
            _Boom0.Play();

            _Boom1.transform.SetParent(null);
            _Boom1.Play();

            _Boom2.transform.SetParent(null);
            _Boom2.Play();

            _Boom3.transform.SetParent(null);
            _Boom3.Play();

            yield return new WaitForSeconds(0.4f);

            //gameObject.SetActive(false);
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<Collider2D>().enabled = false;

            yield return new WaitForSeconds(GlobalData.Inst.BridgeBoomInterval - 0.4f);

            if (_Next != null)
                _Next.Boom();

            if (NetworkServer.active)
                NetworkServer.Destroy(gameObject);
        }
    }
}
