using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Modules.SerializeSystem
{
    public static class SerializeExtensionMethod
    {
        private static Type[] _UnSerializeConstructArgsList = new Type[] { typeof(SerializedClass), typeof(object[]) };

        #region Serialize Extension Methods

        public static SerializedClass Serialize(this sbyte dat, SerializedDataFile sdf)
        {
            return new SerializedClass(dat.ToString(), SerializedObjectType.Basic, sdf);
        }
        public static SerializedClass Serialize(this byte dat, SerializedDataFile sdf)
        {
            return new SerializedClass(dat.ToString(), SerializedObjectType.Basic, sdf);
        }
        public static SerializedClass Serialize(this short dat, SerializedDataFile sdf)
        {
            return new SerializedClass(dat.ToString(), SerializedObjectType.Basic, sdf);
        }
        public static SerializedClass Serialize(this ushort dat, SerializedDataFile sdf)
        {
            return new SerializedClass(dat.ToString(), SerializedObjectType.Basic, sdf);
        }
        public static SerializedClass Serialize(this int dat, SerializedDataFile sdf)
        {
            return new SerializedClass(dat.ToString(), SerializedObjectType.Basic, sdf);
        }
        public static SerializedClass Serialize(this uint dat, SerializedDataFile sdf)
        {
            return new SerializedClass(dat.ToString(), SerializedObjectType.Basic, sdf);
        }
        public static SerializedClass Serialize(this long dat, SerializedDataFile sdf)
        {
            return new SerializedClass(dat.ToString(), SerializedObjectType.Basic, sdf);
        }
        public static SerializedClass Serialize(this ulong dat, SerializedDataFile sdf)
        {
            return new SerializedClass(dat.ToString(), SerializedObjectType.Basic, sdf);
        }
        public static SerializedClass Serialize(this bool dat, SerializedDataFile sdf)
        {
            return new SerializedClass(dat.ToString(), SerializedObjectType.Basic, sdf);
        }
        public static SerializedClass Serialize(this char dat, SerializedDataFile sdf)
        {
            return dat.ToString().Serialize(sdf);
        }
        public static SerializedClass Serialize(this float dat, SerializedDataFile sdf)
        {
            return new SerializedClass(dat.ToString(), SerializedObjectType.Basic, sdf);
        }
        public static SerializedClass Serialize(this double dat, SerializedDataFile sdf)
        {
            return new SerializedClass(dat.ToString(), SerializedObjectType.Basic, sdf);
        }
        public static SerializedClass Serialize(this decimal dat, SerializedDataFile sdf)
        {
            return new SerializedClass(dat.ToString(), SerializedObjectType.Basic, sdf);
        }
        public static SerializedClass Serialize(this Vector2 dat, SerializedDataFile sdf)
        {
            return new SerializedClass($"({dat.x.ToString()},{dat.y.ToString()})", SerializedObjectType.Basic, sdf);
        }
        public static SerializedClass Serialize(this Vector2Int dat, SerializedDataFile sdf)
        {
            return new SerializedClass($"({dat.x.ToString()},{dat.y.ToString()})", SerializedObjectType.Basic, sdf);
        }
        public static SerializedClass Serialize(this Vector3 dat, SerializedDataFile sdf)
        {
            return new SerializedClass($"({dat.x.ToString()},{dat.y.ToString()},{dat.z.ToString()})", SerializedObjectType.Basic, sdf);
        }
        public static SerializedClass Serialize(this Vector3Int dat, SerializedDataFile sdf)
        {
            return new SerializedClass($"({dat.x.ToString()},{dat.y.ToString()},{dat.z.ToString()})", SerializedObjectType.Basic, sdf);
        }
        public static SerializedClass Serialize(this Vector4 dat, SerializedDataFile sdf)
        {
            return new SerializedClass($"({dat.x.ToString()},{dat.y.ToString()},{dat.z.ToString()},{dat.w.ToString()})", SerializedObjectType.Basic, sdf);
        }
        public static SerializedClass Serialize(this Enum dat, SerializedDataFile sdf)
        {
            return new SerializedClass(dat.ToString().Replace(',', '|'), SerializedObjectType.Enum, sdf);
        }
        public static SerializedClass Serialize(this string dat, SerializedDataFile sdf)
        {
            return new SerializedClass($"\"{dat.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t").Replace("\"", "\\\"")}\"",
                SerializedObjectType.String, sdf);
        }

        public static SerializedClass Serialize(this IEnumerable obj, string alias, SerializeConfig cfg, SerializedDataFile sdf)
        {
            if (sdf.SerializeBegin(alias, obj, cfg, out SerializedClass sc))
            {
                int i = 0;
                foreach (object item in obj)
                {
                    GeneralSerializeMethod gsm = SerializeTool.GetGeneralSerializeMethod(item?.GetType());
                    SerializedClass temp_sc = gsm(item,
                                                    cfg.IsElementDelayDefine ? cfg.GetElementAlias(item, i, sdf) : null,
                                                    cfg.IsElementDelayDefine ? new SerializeConfig(SerializeConfigOption.DelayDefine) : SerializeConfig.Default,
                                                    sdf);
                    sc.AddField($"#{i}#", temp_sc, sdf);
                    i++;
                }
                sc.AddField("#Count#", i, sdf);
                sc.AddField("#PreferColumn#", cfg.PreferColumn, sdf);

                //数组类型可能要加特性
                Type t = obj.GetType();
                if (typeof(Array).IsAssignableFrom(t))
                {
                    int rank = t.GetArrayRank();
                    if (rank > 1)
                    {
                        Matrix atb = new Matrix();
                        atb.Rank = rank;
                        for (int j = 0; j < rank; j++)
                            atb.DimensionSize.Add((obj as Array).GetUpperBound(j) + 1);
                        sc.Attributes ??= new List<SerializeAttributeBase>();
                        sc.Attributes.Add(atb);
                    }
                }
            }
            return sdf.SerializeEnd(sc);
        }

        public static SerializedClass Serialize(this ITuple obj, string alias, SerializeConfig cfg, SerializedDataFile sdf)
        {
            if (sdf.SerializeBegin(alias, obj, cfg, out SerializedClass sc))
            {
                for (int i = 0; i < obj.Length; i++)
                    sc.AddField($"#{i}#",
                                obj[i],
                                cfg.IsElementDelayDefine ? cfg.GetElementAlias(obj, i, sdf) : null,
                                cfg.IsElementDelayDefine ? new SerializeConfig(SerializeConfigOption.DelayDefine) : SerializeConfig.Default,
                                sdf);
                sc.AddField("#Count#", obj.Length, sdf);
            }
            return sdf.SerializeEnd(sc);
        }

        #endregion

        /// <summary>
        /// 获取类型的反序列化构造方法
        /// </summary>
        /// <param name="mtd">获取的方法</param>
        /// <param name="isconstructor">是否是一个构造函数（ConstructorInfo）</param>
        /// <param name="noarg">是否无参</param>
        /// <returns>是否成功</returns>
        /// <remarks>
        /// <para>有三种情况：</para>
        /// <para>1.带有<seealso cref="UnSerializeConstruct"/>特性的静态方法</para>
        /// <para>2.具有<seealso cref="SerializedClass"/>和object[]参数的构造方法</para>
        /// <para>3.无参构造方法</para>
        /// </remarks>
        public static bool GetUnSerializeConstruct(this Type typ, out MethodBase mtd, out bool isconstructor, out bool noarg)
        {
            try
            {
                //检查静态方法
                MethodInfo mtd_info = typ.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(x => x.GetCustomAttribute<UnSerializeConstruct>() != null);
                if (mtd_info != null)
                {
                    mtd = mtd_info;
                    isconstructor = false;
                    noarg = false;
                    return true;
                }

                //检查带两个参数的构造方法
                ConstructorInfo cnst_info = typ.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, _UnSerializeConstructArgsList, null);
                if (cnst_info != null)
                {
                    mtd = cnst_info;
                    isconstructor = true;
                    noarg = false;
                    return true;
                }

                //检查无参构造方法
                cnst_info = typ.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, null);
                if (cnst_info != null)
                {
                    mtd = cnst_info;
                    isconstructor = true;
                    noarg = true;
                    return true;
                }
            }
            catch
            {
            }

            mtd = null;
            isconstructor = true;
            noarg = false;

            return false;
        }

    }
}
