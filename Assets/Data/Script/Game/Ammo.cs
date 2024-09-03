using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Contra
{
    public static class Ammo
    {
        public static Dictionary<Gun.Type, GameObject> Prefabs { get; private set; } = new Dictionary<Gun.Type, GameObject>();

        public static void LoadPrefabs()
        {
            Addressables.LoadAssetsAsync("Ammo", (GameObject x) => Prefabs.Add(Enum.Parse<Gun.Type>(x.name), x));
        }
    }
}
