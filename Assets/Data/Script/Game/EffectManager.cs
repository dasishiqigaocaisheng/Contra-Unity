using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Mirror;

public class EffectManager : NetworkBehaviour
{
    public static EffectManager Inst { get; private set; }

    private Dictionary<string, GameObject> _EffectPrefabLUT = new Dictionary<string, GameObject>();

    private void Awake()
    {
        Inst = this;
        Addressables.LoadAssetsAsync<GameObject>("Effect", (x) => _EffectPrefabLUT.Add(x.name, x)).WaitForCompletion();
    }

    [ClientRpc(includeOwner = true)]
    public void PlayEffect(string name, Vector3 pos)
    {
        Instantiate(_EffectPrefabLUT[name], pos, Quaternion.identity).GetComponent<ParticleSystem>().Play();
    }
}
