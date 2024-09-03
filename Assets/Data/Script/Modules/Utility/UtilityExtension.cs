using System.Runtime.CompilerServices;
using UnityEngine;
using Modules.LogSystem;

namespace Modules.Utility
{
    public static class UtilityExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            if (go.GetComponent<T>() == null)
                return go.AddComponent<T>();
            else
                return go.GetComponent<T>();
        }

        /// <summary>
        /// 获取子物体中坐标相对其某个父物体的坐标
        /// </summary>
        /// <param name="v">子物体中的坐标</param>
        /// <param name="child">父物体</param>
        /// <param name="parent">子物体</param>
        /// <returns><paramref name="child"/>坐标系中的坐标<paramref name="v"/>在<paramref name="parent"/>坐标系中的值</returns>
        /// <remarks><paramref name="child"/>必须是<paramref name="parent"/>的子物体或者是其自身</remarks>
        public static Vector3 Relate2Parent(this Vector3 v, Transform child, Transform parent)
        {
            if (child == null) GameException.ArgNullThrow(nameof(child));
            if (parent == null) GameException.ArgNullThrow(nameof(parent));

            Vector3 v_rtn = v;
            Transform tr = child;
            while (tr != null)
            {
                if (tr == parent)
                    return v_rtn;
                else
                {
                    v_rtn += tr.localPosition;
                    tr = tr.parent;
                }
            }

            LogManager.Error("Utility.UtilityExtension", "parent不是child的父物体");
            return v;
        }

        public static Vector2 Abs(this Vector2 dat)
        {
            dat.x = Mathf.Abs(dat.x);
            dat.y = Mathf.Abs(dat.y);
            return dat;
        }

        public static Vector3 Abs(this Vector3 dat)
        {
            dat.x = Mathf.Abs(dat.x);
            dat.y = Mathf.Abs(dat.y);
            dat.z = Mathf.Abs(dat.z);
            return dat;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Abs(this float dat)
        {
            return Mathf.Abs(dat);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Proj2X(this Vector3 v)
        {
            return new Vector3(v.x, 0, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Proj2Y(this Vector3 v)
        {
            return new Vector3(0, v.y, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Proj2Z(this Vector3 v)
        {
            return new Vector3(0, 0, v.z);
        }

        public static Vector3 EightDirQuantize(this Vector3 v)
        {
            Vector3 res = Vector3.zero;
            if (v.x.FEqual(0))
            {
                if (v.y.FEqual(0))
                    return res;
                res.y = 1;
            }
            else
            {
                float tan = Mathf.Abs(v.y / v.x);
                if (tan < 0.414213f)
                    res.x = 1;
                else if (tan < 2.4142135f)
                {
                    res.x = 1;
                    res.y = 1;
                }
                else
                    res.y = 1;

                if (v.x < 0)
                    res.x *= -1;
            }

            if (v.y < 0)
                res.y *= -1;

            return res;
        }

        public static float Slope(this Vector2 v)
        {
            if (v.x.FEqual(0))
            {
                if (v.y.FEqual(0))
                    return float.NaN;
                else if (v.y > 0)
                    return float.PositiveInfinity;
                else
                    return float.NegativeInfinity;
            }
            else
                return v.y / v.x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Round(this float dat)
        {
            return (int)(dat + 0.5f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool FEqual(this float dat0, float dat1)
        {
            return Mathf.Approximately(dat0, dat1);
        }
    }
}
