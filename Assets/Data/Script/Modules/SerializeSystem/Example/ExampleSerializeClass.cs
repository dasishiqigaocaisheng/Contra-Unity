using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.SerializeSystem.Example
{
    /// <summary>
    /// 序列化系统测试类
    /// </summary>
    /// <remarks>
    /// 用法示例：
    /// <code>
    /// <![CDATA[
    /// /************************************序列化过程************************************/
    /// //首先构造数据空间
    /// SerializeDataSpace sds = new SerializeDataSpace();
    /// 
    /// //从数据空间构造序列化文件
    /// SerializedDataFile sdf = sds.CreateSDF("file0");
    /// 
    /// //像文件中写入对象，也就是序列化过程
    /// SerializedClass sc = sdf.WriteObject(esc, "ExampleSerialize", new SerializeConfig());
    /// ... //其它对象的序列化
    /// 
    /// //最后执行后处理
    /// sds.PostProcess();
    /// 
    /// //生成字符串，写入文件
    /// string str = sdf.ToString();
    /// 
    /// /************************************反序列化过程************************************/
    /// //首先构造数据空间
    /// sds = new SerializeDataSpace();
    /// 
    /// //载入序列化文件（首先要将内容读取进来）
    /// sdf = sds.LoadSDF("file0", str);
    /// 
    /// //解析文件
    /// sdf.TryParse();
    /// 
    /// //反序列化
    /// var temp = sdf.Root["ExampleSerialize"].UnSerialize<ExampleSerializeClass>();
    /// ]]>
    /// </code>
    /// </remarks>
    public class ExampleSerializeClass : IAllowSerialize
    {
        public sbyte dat_sbyte;
        public byte dat_byte;
        public short dat_short;
        public ushort dat_ushort;
        public int dat_int1;
        public int dat_int2;
        public uint dat_uint;
        public long dat_long;
        public ulong dat_ulong;
        public float dat_float;
        public double dat_double;
        public bool dat_bool;
        public char dat_char;
        public decimal dat_decimal;
        public string dat_string;
        public Vector2 dat_vector2;
        public Vector2Int dat_vector2int;
        public Vector3 dat_vector3;
        public Vector3Int dat_vector3int;
        public Vector4 dat_vector4;
        public string null_str;
        public int[] dat_array1;
        public int[,] dat_array2;
        public int[] array_ref;
        public Vector2[] dat_array3;
        public List<float> dat_list1;
        public List<float> dat_list2;
        public Dictionary<string, int> dat_dic;
        public Dictionary<string, string> dat_externref;
        public (string, int, float[]) tuple;
        public ExampleSerializeClass2 example_class;

        public void Set()
        {
            dat_sbyte = 0;
            dat_byte = 201;
            dat_short = 215;
            dat_ushort = 65535;
            dat_int1 = 256;
            dat_int2 = -54225;
            dat_uint = 512565852;
            dat_long = -21540236112;
            dat_ulong = 21540236112;
            dat_float = 2103.1251f;
            dat_double = -1.021458965;
            dat_bool = true;
            dat_char = 'c';
            dat_decimal = 2012.0712M;
            dat_string = "\tThis is an example.";
            dat_vector2 = new Vector2(0, 0);
            dat_vector2int = new Vector2Int(15, 23);
            dat_vector3 = new Vector3(23, 56, 89);
            dat_vector3int = new Vector3Int(1, 5, 9);
            dat_vector4 = new Vector4(-10.0f, 56.125f, -4.25f, 21.2f);
            null_str = null;
            dat_array1 = new int[] { 1, 2, 3, 4, 5, 6 };
            dat_array2 = new int[,]{ {1, 2, 3, 4, 5 },
                                    {6,7,8,9,10},
                                    {11,12,13,14,15}};
            array_ref = dat_array1;
            dat_array3 = new Vector2[] { Vector2.down, Vector2.left, Vector2.negativeInfinity, Vector2.one, Vector2.positiveInfinity };
            dat_list1 = new List<float>() { 10.2f, 32f, 25.25f };
            dat_list2 = new List<float>() { float.Epsilon, float.MaxValue, float.MinValue, float.NaN, float.NegativeInfinity, float.PositiveInfinity };
            dat_dic = new Dictionary<string, int>() { { "max", int.MaxValue }, { "min", int.MinValue } };
            dat_externref = new Dictionary<string, string>() { { "China", "Beijing" }, { "Britain", "London" }, { "Ukrain", "Kyiv" } };
            tuple = ("Tuple", 2, new float[] { 1.2f, 2.3f, 3.4f });
            example_class = new ExampleSerializeClass2();
            example_class.Set();
            example_class.Master = this;
        }

        public SerializedClass Serialize(string alias, SerializeConfig cfg, SerializedDataFile sdf)
        {
            if (sdf.SerializeBegin(alias, this, cfg, out SerializedClass sc))
            {
                sc.AddField("dat_sbyte", dat_sbyte, sdf);
                sc.AddField("dat_byte", dat_byte, sdf);
                sc.AddField("dat_short", dat_short, sdf);
                sc.AddField("dat_ushort", dat_ushort, sdf);
                sc.AddField("dat_int1", dat_int1, sdf);
                sc.AddField("dat_int2", dat_int2, sdf);
                sc.AddField("dat_uint", dat_uint, sdf);
                sc.AddField("dat_long", dat_long, sdf);
                sc.AddField("dat_ulong", dat_ulong, sdf);
                sc.AddField("dat_float", dat_float, sdf);
                sc.AddField("dat_double", dat_double, sdf);
                sc.AddField("dat_bool", dat_bool, sdf);
                sc.AddField("dat_char", dat_char, sdf);
                sc.AddField("dat_decimal", dat_decimal, sdf);
                sc.AddField("dat_string", dat_string, sdf);
                sc.AddField("dat_vector2", dat_vector2, sdf);
                sc.AddField("dat_vector2int", dat_vector2int, sdf);
                sc.AddField("dat_vector3", dat_vector3, sdf);
                sc.AddField("dat_vector3int", dat_vector3int, sdf);
                sc.AddField("dat_vector4", dat_vector4, sdf);
                sc.AddField("null_str", null_str, sdf);
                sc.AddField("dat_array1", dat_array1, sdf);
                sc.AddField("dat_array2", dat_array2, sdf);
                sc.AddField("array_ref", array_ref, sdf);
                sc.AddField("dat_array3", dat_array3, new SerializeConfig(SerializeConfigOption.DelayDefine), sdf);
                sc.AddField("dat_list1", dat_list1, sdf);
                sc.AddField("dat_list2", dat_list2, new SerializeConfig(SerializeConfigOption.DelayDefine), sdf);
                sc.AddField("dat_dic", dat_dic, sdf);
                sc.AddField("dat_externref", dat_externref, SerializeConfig.Default, sdf);
                sc.AddField("tuple", tuple, sdf);
                sc.AddField("example_class", example_class, sdf);
            }
            return sdf.SerializeEnd(sc);
        }

        public object UnSerializeOverWrite(SerializedClass sc, UnSerializeOption opt)
        {
            dat_sbyte = sc["dat_sbyte"].UnSerialize<sbyte>(opt);
            dat_byte = sc["dat_byte"].UnSerialize<byte>(opt);
            dat_short = sc["dat_short"].UnSerialize<short>(opt);
            dat_ushort = sc["dat_ushort"].UnSerialize<ushort>(opt);
            dat_int1 = sc["dat_int1"].UnSerialize<int>(opt);
            dat_int2 = sc["dat_int2"].UnSerialize<int>(opt);
            dat_uint = sc["dat_uint"].UnSerialize<uint>(opt);
            dat_long = sc["dat_long"].UnSerialize<long>(opt);
            dat_ulong = sc["dat_ulong"].UnSerialize<ulong>(opt);
            dat_float = sc["dat_float"].UnSerialize<float>(opt);
            dat_double = sc["dat_double"].UnSerialize<double>(opt);
            dat_bool = sc["dat_bool"].UnSerialize<bool>(opt);
            dat_char = sc["dat_char"].UnSerialize<char>(opt);
            dat_decimal = sc["dat_decimal"].UnSerialize<decimal>(opt);
            dat_string = sc["dat_string"].UnSerialize<string>(opt);
            dat_vector2 = sc["dat_vector2"].UnSerialize<Vector2>(opt);
            dat_vector2int = sc["dat_vector2int"].UnSerialize<Vector2Int>(opt);
            dat_vector3 = sc["dat_vector3"].UnSerialize<Vector3>(opt);
            dat_vector3int = sc["dat_vector3int"].UnSerialize<Vector3Int>(opt);
            dat_vector4 = sc["dat_vector4"].UnSerialize<Vector4>(opt);
            null_str = sc["null_str"].UnSerialize<string>(opt);
            dat_array1 = sc["dat_array1"].UnSerialize<int[]>(opt);
            dat_array2 = sc["dat_array2"].UnSerialize<int[,]>(opt);
            array_ref = sc["array_ref"].UnSerialize<int[]>(opt);
            dat_array3 = sc["dat_array3"].UnSerialize<Vector2[]>(opt);
            dat_list1 = sc["dat_list1"].UnSerialize<List<float>>(opt);
            dat_list2 = sc["dat_list2"].UnSerialize<List<float>>(opt);
            dat_dic = sc["dat_dic"].UnSerialize<Dictionary<string, int>>(opt);
            dat_externref = sc["dat_externref"].UnSerialize<Dictionary<string, string>>(opt);
            tuple = sc["tuple"].UnSerialize<ValueTuple<string, int, float[]>>(opt);
            example_class = sc["example_class"].UnSerialize<ExampleSerializeClass2>(opt, this);

            return this;
        }
    }

    public class ExampleSerializeClass2 : IAllowSerialize
    {
        public int dat_int;
        public List<Vector2[]> dat_vectorlist;
        public ExampleSerializeClass Master;

        public void Set()
        {
            dat_int = -712;
            dat_vectorlist = new List<Vector2[]>()
                {
                    new Vector2[] { new Vector2(1, 2), new Vector2(4,5), new Vector2(2,6)},
                    new Vector2[] { new Vector2(1, 2), new Vector2(4,5), new Vector2(2,-2), new Vector2(21,232), new Vector2(2,6)},
                    new Vector2[] { new Vector2(1, 2), new Vector2(4,5), new Vector2(1,1), new Vector2(0,0)},
                    new Vector2[] { new Vector2(5, 5), new Vector2(122,-845)},
                };

        }

        [UnSerializeConstruct]
        private static ExampleSerializeClass2 _UnSerializeConstruct(SerializedClass sc, params object[] args)
        {
            var inst = new ExampleSerializeClass2();
            inst.Master = args[0] as ExampleSerializeClass;
            return inst;
        }

        public SerializedClass Serialize(string alias, SerializeConfig cfg, SerializedDataFile sdf)
        {
            if (sdf.SerializeBegin(alias, this, cfg, out SerializedClass sc))
            {
                sc.AddField("dat_int", dat_int, sdf);
                sc.AddField("dat_vectorlist", dat_vectorlist, sdf);
                sc.AddField("master", Master, sdf);
            }
            return sdf.SerializeEnd(sc);
        }

        public object UnSerializeOverWrite(SerializedClass sc, UnSerializeOption opt)
        {
            dat_int = sc["dat_int"].UnSerialize<int>(opt);
            dat_vectorlist = sc["dat_vectorlist"].UnSerialize<List<Vector2[]>>(opt);
            Master = sc["master"].UnSerialize<ExampleSerializeClass>(opt);

            return this;
        }
    }
}
