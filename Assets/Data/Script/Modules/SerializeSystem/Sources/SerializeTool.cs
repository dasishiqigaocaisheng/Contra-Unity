using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Modules.LogSystem;

namespace Modules.SerializeSystem
{
    //通用序列化方法
    public delegate SerializedClass GeneralSerializeMethod(object dat, string alias, SerializeConfig cfg, SerializedDataFile sdf);

    //通用反序列化方法
    public delegate object GeneralUnSerializeMethod(SerializedClass sc, Type typ, UnSerializeOption opt, params object[] args);

    /*
     *序列化工具
     */
    public static class SerializeTool
    {
        //包含一些单值类型的序列化方法
        //x：obj
        //y：alias
        //z：cfg
        //w：sds
        static readonly Dictionary<Type, GeneralSerializeMethod> _AllSerializeMethod = new Dictionary<Type, GeneralSerializeMethod>
        {
            {typeof(sbyte),(x,y,z,w)=>((sbyte)x).Serialize(w)},
            {typeof(byte),(x,y,z, w)=>((byte) x).Serialize(w)},
            {typeof(short),(x,y,z, w)=>((short) x).Serialize(w)},
            {typeof(ushort),(x,y,z, w)=>((ushort) x).Serialize(w)},
            {typeof(int),(x,y,z, w)=>((int) x).Serialize(w)},
            {typeof(uint),(x,y,z, w)=>((uint) x).Serialize(w)},
            {typeof(long),(x,y,z, w)=>((long) x).Serialize(w)},
            {typeof(ulong),(x,y,z, w)=>((ulong) x).Serialize(w)},
            {typeof(bool),(x,y,z, w)=>((bool) x).Serialize(w)},
            {typeof(char),(x,y,z, w)=>((char) x).Serialize(w)},
            {typeof(float),(x,y,z, w)=>((float) x).Serialize(w)},
            {typeof(double),(x,y,z, w)=>((double) x).Serialize(w)},
            {typeof(decimal),(x,y,z, w)=>((decimal) x).Serialize(w)},
            {typeof(Vector2),(x,y,z, w)=>((Vector2) x).Serialize(w)},
            {typeof(Vector2Int),(x,y,z, w)=>((Vector2Int) x).Serialize(w)},
            {typeof(Vector3),(x,y,z, w)=>((Vector3) x).Serialize(w)},
            {typeof(Vector3Int),(x,y,z, w)=>((Vector3Int) x).Serialize(w)},
            {typeof(Vector4),(x,y,z, w)=>((Vector4) x).Serialize(w)},
            {typeof(string),(x,y,z, w)=>((string) x).Serialize(w)},
            {typeof(SerializedClass),(x,y,z, w)=>((SerializedClass)x)}
        };

        private static SerializedClass _KeyValuePairSerialize(object kvp, string alias, SerializeConfig cfg, SerializedDataFile sdf)
        {
            if (sdf.SerializeBegin(alias, kvp, cfg, out SerializedClass sc))
            {
                GeneralSerializeMethod gsm = SerializeTool.GetGeneralSerializeMethod(((dynamic)kvp).Key?.GetType());
                SerializedClass temp_sc = gsm(((dynamic)kvp).Key, cfg.IsKeyDelayDefine ? cfg.GetKeyAlias(kvp, sdf) : null, cfg, sdf);
                sc.AddField("#Key#", temp_sc, sdf);

                gsm = SerializeTool.GetGeneralSerializeMethod(((dynamic)kvp).Value?.GetType());
                temp_sc = gsm(((dynamic)kvp).Value, cfg.IsValueDelayDefine ? cfg.GetValueAlias(kvp, sdf) : null, cfg, sdf);
                sc.AddField("#Value#", temp_sc, sdf);
            }

            return sdf.SerializeEnd(sc);
        }

        /*
         * 功能：根据类型获取一个相应的序列化方法
         *参数：
         *  1.t：类型
         *返回值：
         *  相对应的序列化方法，没有找到则返回null
         */
        public static GeneralSerializeMethod GetGeneralSerializeMethod(Type t)
        {
            if (t == null)
                return new GeneralSerializeMethod((x, y, z, w) => new SerializedClass(null, SerializedObjectType.Null, w));

            if (_AllSerializeMethod.TryGetValue(t, out GeneralSerializeMethod gsm))
                return gsm;
            else
            {
                if (typeof(IAllowSerialize).IsAssignableFrom(t))
                    return new GeneralSerializeMethod((x, y, z, w) => (x as IAllowSerialize).Serialize(y, z, w));
                else if (typeof(Array).IsAssignableFrom(t))
                    return new GeneralSerializeMethod((x, y, z, w) => (x as IEnumerable).Serialize(y, z, w));
                else if (typeof(IDictionary).IsAssignableFrom(t))
                    return new GeneralSerializeMethod((x, y, z, w) => (x as IEnumerable).Serialize(y, z, w));
                else if (typeof(IList).IsAssignableFrom(t))
                    return new GeneralSerializeMethod((x, y, z, w) => (x as IEnumerable).Serialize(y, z, w));
                else if (typeof(IEnumerable).IsAssignableFrom(t))
                    return new GeneralSerializeMethod((x, y, z, w) => (x as IEnumerable).Serialize(y, z, w));
                else if (t.IsGenericType)
                {
                    if (t.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                        return new GeneralSerializeMethod((x, y, z, w) => _KeyValuePairSerialize(x, y, z, w));
                    else if (typeof(ITuple).IsAssignableFrom(t) && t.IsValueType)
                        return new GeneralSerializeMethod((x, y, z, w) => (x as ITuple).Serialize(y, z, w));
                    else
                    {
                        LogManager.Error("SerializeSystem", $"找不到对应泛型的序列化方法：{t.GetGenericTypeDefinition()}");
                        return new GeneralSerializeMethod((x, y, z, w) => new SerializedClass(null, SerializedObjectType.Error, w));
                    }
                }
                else if (t.IsEnum)
                    return new GeneralSerializeMethod((x, y, z, w) => (x as Enum).Serialize(w));
                else
                {
                    LogManager.Error("SerializeSystem", $"找不到对应类型的序列化方法：{t.GetGenericTypeDefinition()}");
                    return new GeneralSerializeMethod((x, y, z, w) => new SerializedClass(null, SerializedObjectType.Error, w));
                }
            }
        }


        //包含一些单值类型的反序列化方法
        static readonly Dictionary<Type, GeneralUnSerializeMethod> _AllUnSerializeMethod = new Dictionary<Type, GeneralUnSerializeMethod>
        {
            {typeof(sbyte),(x,y,z,w)=>_UnSerialize2Sbyte(x)},
            {typeof(byte),(x,y,z,w)=>_UnSerialize2Byte(x)},
            {typeof(short),(x,y,z,w)=>_UnSerialize2Short(x)},
            {typeof(ushort),(x,y,z,w)=>_UnSerialize2Ushort(x)},
            {typeof(int),(x,y,z,w)=>_UnSerialize2Int(x)},
            {typeof(uint),(x,y,z,w)=>_UnSerialize2Uint(x)},
            {typeof(long),(x,y,z,w)=>_UnSerialize2Long(x)},
            {typeof(ulong),(x,y,z,w)=>_UnSerialize2Ulong(x)},
            {typeof(bool),(x,y,z,w)=>_UnSerialize2Bool(x)},
            {typeof(char),(x,y,z,w)=>_UnSerialize2Char(x)},
            {typeof(float),(x,y,z,w)=>_UnSerialize2Float(x)},
            {typeof(double),(x,y,z,w)=>_UnSerialize2Double(x)},
            {typeof(decimal),(x,y,z,w)=>_UnSerialize2Decimal(x)},
            {typeof(Vector2),(x,y,z,w)=>_UnSerialize2Vector2(x)},
            {typeof(Vector2Int),(x,y,z,w)=>_UnSerialize2Vector2Int(x)},
            {typeof(Vector3),(x,y,z,w)=>_UnSerialize2Vector3(x)},
            {typeof(Vector3Int),(x,y,z,w)=>_UnSerialize2Vector3Int(x)},
            {typeof(Vector4),(x,y,z,w)=>_UnSerialize2Vector4(x)},
            {typeof(string),(x,y,z,w)=>_UnSerialize2String(x)},
            {typeof(SerializedClass),(x,y,z,w)=>x}
        };

        /*
         * 功能：根据类型获取一个相应的反序列化方法
         *参数：
         *  1.t：类型
         *返回值：
         *  相对应的反序列化方法，没有找到则返回null
         */
        public static GeneralUnSerializeMethod GetGeneralUnSerializeMethod(Type t)
        {
            if (_AllUnSerializeMethod.TryGetValue(t, out GeneralUnSerializeMethod gum))
                return gum;
            else
            {
                if (typeof(IAllowSerialize).IsAssignableFrom(t))
                    return new GeneralUnSerializeMethod((x, y, z, w) =>
                    {
                        if (y.GetUnSerializeConstruct(out MethodBase mtdb, out bool isconstructor, out bool noarg))
                        {
                            IAllowSerialize inst = (isconstructor, noarg) switch
                            {
                                (true, true) => (IAllowSerialize)(mtdb as ConstructorInfo).Invoke(null),
                                (true, false) => (IAllowSerialize)(mtdb as ConstructorInfo).Invoke(new object[] { x, w }),
                                (false, _) => (IAllowSerialize)mtdb.Invoke(null, new object[] { x, w })
                            };

                            if (inst != null)
                            {
                                if (x.RefCount != 0)
                                {
                                    if (x.CopyCount == 0)
                                        x.SDF.Space.AddObject(inst, x);
                                    else
                                        x.SDF.Space.CopyIncludeObjects.AddOrUpdate(inst, (x, x.CopyCount));
                                }
                                return inst.UnSerializeOverWrite(x, z);
                            }
                            else
                            {
                                LogManager.Error("SerializeSystem.SerializeTool", "反序列化构造结果为null",
                                    args: new (string, object)[] { ("SC路径", x.GetPath()), ("类型", y) });
                                return null;
                            }
                        }
                        LogManager.Error("SerializeSystem.SerializeTool", $"找不到类型（{y}）的反序列化构造方法", args: ("SC路径", x.GetPath()));
                        return null;
                    });
                else if (t.IsArray)
                    return new GeneralUnSerializeMethod((x, y, z, w) => _UnSerialize2Array(x, y, z, w));
                else if (t.IsGenericType)
                {
                    Type gt = t.GetGenericTypeDefinition();
                    if (gt == typeof(List<>))
                        return new GeneralUnSerializeMethod((x, y, z, w) => _UnSerialize2List(x, y, z, w));
                    else if (gt == typeof(Dictionary<,>))
                        return new GeneralUnSerializeMethod((x, y, z, w) => _UnSerialize2Dictionary(x, y, z, w));
                    //反序列化时不会考虑KeyValuePair<,>类型，因为字典类型直接在_UnSerialize2Dictionary中完成反序列化
                    //else if (gt == typeof(KeyValuePair<,>))
                    //    return new GeneralUnSerializeMethod((x, y, z) => _UnSerialize2Dictionary(x, y, z));
                    else if (typeof(ITuple).IsAssignableFrom(t))
                        return new GeneralUnSerializeMethod((x, y, z, w) => _UnSerialize2Tuple(x, y, z, w));
                    else
                    {
                        LogManager.Error("SerializeSystem", $"找不到对应泛型的反序列化方法：{gt}");
                        return null;
                    }
                }
                //else if (typeof(IEnumerable).IsAssignableFrom(t))
                //{
                //    
                //}
                else if (typeof(Enum).IsAssignableFrom(t))
                    return new GeneralUnSerializeMethod((x, y, z, w) => _UnSerialize2Enum(x, t));
                else
                {
                    LogManager.Error("SerializeSystem", $"找不到对应类型的反序列化方法：{t}");
                    return null;
                }
            }
        }


        private static sbyte _UnSerialize2Sbyte(SerializedClass sc)
        {
            if (sbyte.TryParse(sc.Alias, out sbyte result))
                return result;
            else
            {
                LogManager.Error("SerializeSystem", $"将字段{sc.GetPath()}反序列化为sbyte时解析出错");
                return default;
            }
        }

        private static byte _UnSerialize2Byte(SerializedClass sc)
        {
            if (byte.TryParse(sc.Alias, out byte result))
                return result;
            else
            {
                LogManager.Error("SerializeSystem", $"将字段{sc.GetPath()}反序列化为byte时解析出错");
                return default;
            }
        }

        private static short _UnSerialize2Short(SerializedClass sc)
        {
            if (short.TryParse(sc.Alias, out short result))
                return result;
            else
            {
                LogManager.Error("SerializeSystem", $"将字段{sc.GetPath()}反序列化为short时解析出错");
                return default;
            }
        }

        private static ushort _UnSerialize2Ushort(SerializedClass sc)
        {
            if (ushort.TryParse(sc.Alias, out ushort result))
                return result;
            else
            {
                LogManager.Error("SerializeSystem", $"将字段{sc.GetPath()}反序列化为ushort时解析出错");
                return default;
            }
        }

        private static int _UnSerialize2Int(SerializedClass sc)
        {
            if (int.TryParse(sc.Alias, out int result))
                return result;
            else
            {
                LogManager.Error("SerializeSystem", $"将字段{sc.GetPath()}反序列化为int时解析出错");
                return default;
            }
        }

        private static uint _UnSerialize2Uint(SerializedClass sc)
        {
            if (uint.TryParse(sc.Alias, out uint result))
                return result;
            else
            {
                LogManager.Error("SerializeSystem", $"将字段{sc.GetPath()}反序列化为uint时解析出错");
                return default;
            }
        }

        private static long _UnSerialize2Long(SerializedClass sc)
        {
            if (long.TryParse(sc.Alias, out long result))
                return result;
            else
            {
                LogManager.Error("SerializeSystem", $"将字段{sc.GetPath()}反序列化为long时解析出错");
                return default;
            }
        }

        private static ulong _UnSerialize2Ulong(SerializedClass sc)
        {
            if (ulong.TryParse(sc.Alias, out ulong result))
                return result;
            else
            {
                LogManager.Error("SerializeSystem", $"将字段{sc.GetPath()}反序列化为ulong时解析出错");
                return default;
            }
        }

        private static bool _UnSerialize2Bool(SerializedClass sc)
        {
            if (bool.TryParse(sc.Alias, out bool result))
                return result;
            else
            {
                LogManager.Error("SerializeSystem", $"将字段{sc.GetPath()}反序列化为bool时解析出错");
                return default;
            }
        }

        private static char _UnSerialize2Char(SerializedClass sc)
        {
            if (sc.Alias.Length > 2)
                return sc.Alias[1];
            else
            {
                LogManager.Error("SerializeSystem", $"将字段{sc.GetPath()}反序列化为char时解析出错");
                return default;
            }
        }

        private static float _UnSerialize2Float(SerializedClass sc)
        {
            if (float.TryParse(sc.Alias, out float result))
                return result;
            else
            {
                LogManager.Error("SerializeSystem", $"将字段{sc.GetPath()}反序列化为float时解析出错");
                return default;
            }
        }

        private static double _UnSerialize2Double(SerializedClass sc)
        {
            if (double.TryParse(sc.Alias, out double result))
                return result;
            else
            {
                LogManager.Error("SerializeSystem", $"将字段{sc.GetPath()}反序列化为double时解析出错");
                return default;
            }
        }

        private static decimal _UnSerialize2Decimal(SerializedClass sc)
        {
            if (decimal.TryParse(sc.Alias, out decimal result))
                return result;
            else
            {
                LogManager.Error("SerializeSystem", $"将字段{sc.GetPath()}反序列化为decimal时解析出错");
                return default;
            }
        }

        private static Vector2 _UnSerialize2Vector2(SerializedClass sc)
        {
            //获取数字并去除空格
            string[] numbers = sc.Alias.Split(',', StringSplitOptions.RemoveEmptyEntries);

            if (numbers.Length != 2)
            {
                LogManager.Error("SerializeSystem", $"将字段{sc.GetPath()}反序列化为Vector2时出错（维度为{numbers.Length}而不是2）");
                return default;
            }

            Vector2 result;
            if (float.TryParse(numbers[0].Replace("(", ""), out result.x) && float.TryParse(numbers[1].Replace(")", ""), out result.y))
                return result;
            else
            {
                LogManager.Error("SerializeSystem", $"将字段{sc.GetPath()}反序列化为Vector2时解析出错");
                return default;
            }
        }

        private static Vector2Int _UnSerialize2Vector2Int(SerializedClass sc)
        {
            //获取数字并去除空格
            string[] numbers = sc.Alias.Split(',', StringSplitOptions.RemoveEmptyEntries);

            if (numbers.Length != 2)
            {
                LogManager.Error("SerializeSystem", $"将字段{sc.GetPath()}反序列化为Vector2Int时出错（维度为{numbers.Length}而不是2）");
                return default;
            }

            if (int.TryParse(numbers[0].Replace("(", ""), out int x) && int.TryParse(numbers[1].Replace(")", ""), out int y))
                return new Vector2Int(x, y);
            else
            {
                LogManager.Error("SerializeSystem", $"将字段{sc.GetPath()}反序列化为Vector2Int时解析出错");
                return default;
            }
        }

        private static Vector3 _UnSerialize2Vector3(SerializedClass sc)
        {
            //获取数字并去除空格
            string[] numbers = sc.Alias.Split(',', StringSplitOptions.RemoveEmptyEntries);

            if (numbers.Length != 3)
            {
                LogManager.Error("SerializeSystem", $"将字段{sc.GetPath()}反序列化为Vector3时出错（维度为{numbers.Length}而不是3）");
                return default;
            }

            Vector3 result;
            if (float.TryParse(numbers[0].Replace("(", ""), out result.x) && float.TryParse(numbers[1], out result.y) && float.TryParse(numbers[2].Replace(")", ""), out result.z))
                return result;
            else
            {
                LogManager.Error("SerializeSystem", $"将字段{sc.GetPath()}反序列化为Vector3时解析出错");
                return default;
            }
        }

        private static Vector3Int _UnSerialize2Vector3Int(SerializedClass sc)
        {
            //获取数字并去除空格
            string[] numbers = sc.Alias.Split(',', StringSplitOptions.RemoveEmptyEntries);

            if (numbers.Length != 3)
            {
                LogManager.Error("SerializeSystem", $"将字段{sc.GetPath()}反序列化为Vector3Int时出错（维度为{numbers.Length}而不是3）");
                return default;
            }

            if (int.TryParse(numbers[0].Replace("(", ""), out int x) && int.TryParse(numbers[1], out int y) && int.TryParse(numbers[2].Replace(")", ""), out int z))
                return new Vector3Int(x, y, z);
            else
            {
                LogManager.Error("SerializeSystem", $"将字段{sc.GetPath()}反序列化为Vector3Int时解析出错");
                return default;
            }
        }

        private static Vector4 _UnSerialize2Vector4(SerializedClass sc)
        {
            //获取数字并去除空格
            string[] numbers = sc.Alias.Split(',', StringSplitOptions.RemoveEmptyEntries);

            if (numbers.Length != 4)
            {
                LogManager.Error("SerializeSystem", $"将字段{sc.GetPath()}反序列化为Vector4时出错（维度为{numbers.Length}而不是4）");
                return default;
            }

            Vector4 result;
            if (float.TryParse(numbers[0].Replace("(", ""), out result.x) && float.TryParse(numbers[1], out result.y) &&
                float.TryParse(numbers[2], out result.z) && float.TryParse(numbers[3].Replace(")", ""), out result.w))
                return result;
            else
            {
                LogManager.Error("SerializeSystem", $"将字段{sc.GetPath()}反序列化为Vector4时解析出错");
                return default;
            }
        }

        private static Enum _UnSerialize2Enum(SerializedClass sc, Type t)
        {
            //Parse方法不会处理异常
            //所以使用try-catch手动捕获
            try
            {
                return (Enum)Enum.Parse(t, sc.Alias.Replace('|', ','));
            }
            catch (Exception ex)
            {
                LogManager.Error("SerializeSystem", $"将字段{sc.GetPath()}反序列化为Enum（{t}）时解析出错", args: ("异常信息", ex.Message));
                return default;
            }
        }

        private static string _UnSerialize2String(SerializedClass sc)
        {
            string str = sc.Alias;
            if (str[0] == '\"' && str[^1] == '\"')
                return str[1..^1].Replace("\\\\", "\\").Replace("\\r", "\r").Replace("\\n", "\n").Replace("\\t", "\t");
            else
            {
                LogManager.Error("SerializeSystem", $"将字段{sc.GetPath()}反序列化为string时解析出错");
                return default;
            }
        }

        //TODO:对Enumerator类型的反序列化过程需要优化
        private static object _UnSerialize2Array(SerializedClass sc, Type typ, UnSerializeOption opt, params object[] args)
        {
            Matrix atb = sc.Attributes?.Find(x => x is Matrix) as Matrix;
            int len = sc.ObjectType.HasFlag(SerializedObjectType.Empty) ? 0 : _UnSerialize2Int(sc["#Count#"]);
            int rank = atb == null ? 1 : atb.Rank;
            int[] dim_len = new int[rank];
            //获取各维度长度
            for (int i = 0; i < rank; i++)
                dim_len[i] = atb == null ? len : atb.DimensionSize[i];

            Type ele_typ = typ.GetElementType();
            Array array = Array.CreateInstance(ele_typ, dim_len);

            if (sc.RefCount != 0)
            {
                if (sc.CopyCount == 0)
                    sc.SDF.Space.AddObject(array, sc);
                else
                    sc.SDF.Space.CopyIncludeObjects.AddOrUpdate(array, (sc, sc.CopyCount));
            }

            /*Type _ele_typ = ele_typ;
            bool ele_infer = false;
            GeneralUnSerializeMethod gum = null;
            if (sc.HasAttribute<ElementTypeInfer>(out _))
                ele_infer = true;
            else
            {
                gum = GetGeneralUnSerializeMethod(ele_typ);
                if (gum == null)
                    return null;
            }*/

            int cnt = 0;
            int[] dim_idx = new int[rank];
            while (dim_len[0] > dim_idx[0])
            {
                /*SerializedClass ele_sc = sc[cnt++];
                //类型自动推断
                if (ele_infer)
                {
                    //类型重载
                    (gum, _ele_typ) = ele_sc.HasAttribute(out TypeDef td) ?
                        (GetGeneralUnSerializeMethod(td.GetTypeInst()), td.GetTypeInst()) :
                        (GetGeneralUnSerializeMethod(ele_typ), ele_typ);

                    if (gum == null)
                        return null;
                }

                object temp = gum(ele_sc, _ele_typ, opt);
                array.SetValue(temp, dim_idx);*/
                array.SetValue(sc[cnt++].UnSerialize(ele_typ, opt, args), dim_idx);

                //最后一维索引+1
                dim_idx[rank - 1]++;
                //这里判断进位
                for (int i = rank - 1; i >= 0; i--)
                {
                    if (dim_idx[i] >= dim_len[i])
                    {
                        if (i != 0)
                        {
                            dim_idx[i] = 0;
                            dim_idx[i - 1]++;
                        }
                        else
                            break;
                    }
                }
            }

            return array;
        }

        private static object _UnSerialize2List(SerializedClass sc, Type typ, UnSerializeOption opt, params object[] args)
        {
            int len = sc.ObjectType.HasFlag(SerializedObjectType.Empty) ? 0 : _UnSerialize2Int(sc["#Count#"]);

            Type gt = typ.GetGenericArguments()[0];
            IList list = Activator.CreateInstance(typ) as IList;

            if (typ.IsClass && sc.RefCount != 0)
            {
                if (sc.CopyCount == 0)
                    sc.SDF.Space.AddObject(list, sc);
                else
                    sc.SDF.Space.CopyIncludeObjects.AddOrUpdate(list, (sc, sc.CopyCount));
            }


            /*Type ele_typ = gt;
            bool ele_infer = false;
            GeneralUnSerializeMethod gum = null;
            if (sc.HasAttribute<ElementTypeInfer>(out _))
                ele_infer = true;
            else
            {
                gum = GetGeneralUnSerializeMethod(gt);
                if (gum == null)
                    return null;
            }*/

            for (int i = 0; i < len; i++)
            {
                /*SerializedClass ele_sc = sc[i];
                //自动推断
                if (ele_infer)
                {
                    //确实存在推断
                    (gum, ele_typ) = ele_sc.HasAttribute(out TypeDef td) ?
                        (GetGeneralUnSerializeMethod(td.GetTypeInst()), td.GetTypeInst()) :
                        (GetGeneralUnSerializeMethod(ele_typ), ele_typ);

                    if (gum == null)
                        return null;
                }

                object temp = gum(ele_sc, ele_typ, opt);*/
                list.Add(sc[i].UnSerialize(gt, opt, args));
            }

            return list;
        }

        private static IDictionary _UnSerialize2Dictionary(SerializedClass sc, Type typ, UnSerializeOption opt, params object[] args)
        {
            int len = sc.ObjectType.HasFlag(SerializedObjectType.Empty) ? 0 : _UnSerialize2Int(sc["#Count#"]);

            Type[] gts = typ.GetGenericArguments();
            IDictionary dictionary = Activator.CreateInstance(typ) as IDictionary;

            if (typ.IsClass && sc.RefCount != 0)
            {
                if (sc.CopyCount == 0)
                    sc.SDF.Space.AddObject(dictionary, sc);
                else
                    sc.SDF.Space.CopyIncludeObjects.AddOrUpdate(dictionary, (sc, sc.CopyCount));
            }

            GeneralUnSerializeMethod key_gum = GetGeneralUnSerializeMethod(gts[0]);
            GeneralUnSerializeMethod value_gum = GetGeneralUnSerializeMethod(gts[1]);
            if (key_gum is null)
            {
                LogManager.Error("SerializeSystem", $"未能得到Key的对应类型（{gts[0]}）的反序列化方法（key_gum）{sc.GetPath()}");
                return null;
            }
            if (value_gum is null)
            {
                LogManager.Error("SerializeSystem", $"未能得到Value的对应类型（{gts[1]}）的反序列化方法（value_gum）{sc.GetPath()}");
                return null;
            }

            for (int i = 0; i < len; i++)
            {
                SerializedClass kvp = sc[i];
                object key = key_gum(kvp["#Key#"], gts[0], opt, args);
                object value = value_gum(kvp["#Value#"], gts[1], opt, args);
                dictionary.Add(key, value);
            }

            return dictionary;
        }

        private static ITuple _UnSerialize2Tuple(SerializedClass sc, Type t, UnSerializeOption opt, params object[] args)
        {
            int item_num = _UnSerialize2Int(sc["#Count#"]);
            object[] items = new object[item_num];

            Type[] gargs = t.GetGenericArguments();

            if (gargs.Length != item_num)
            {
                LogManager.Error("SerializeSystem", $"泛型个数（{gargs.Length}）与元组元素（{item_num}）数目不一致");
                return null;
            }

            for (int i = 0; i < gargs.Length; i++)
                items[i] = sc[i].UnSerialize(gargs[i], opt, args);

            return Activator.CreateInstance(t, items) as ITuple;
        }
    }
}
