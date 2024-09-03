using UnityEngine;

namespace Modules.Utility
{
    public static class Util
    {
        public static Vector3 GetLocalPosition(Transform father, Transform child)
        {
            return child.position - father.position;
        }
    }
}
