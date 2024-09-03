using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Modules.LogSystem;
using Modules.Utility.BidirectionalDict;

namespace Modules.SerializeSystem
{
    public class SerializeDataSpace
    {
        //命名计数器
        private uint _AliasCounter;

        //获取下一个别名
        internal string NextAlias { get => $"__Object{_AliasCounter++}"; }

        //此数据空间包含的对象
        private BiDictionary<object, SerializedClass> _IncludedObjects = new BiDictionary<object, SerializedClass>();

        //该数据空间中所有序列化文件
        private readonly List<SerializedDataFile> _SDFs = new List<SerializedDataFile>();

        //搁置的引用对象
        //用于处理跨SDF的对象引用问题
        //标记为Extern的对象在序列化时，如果在_IncludedObjects没有找到引用对象的记录，则会搁置
        private readonly List<(SerializedClass, object)> _ShelveObject0 = new List<(SerializedClass, object)>();

        //当复制一个字段时，提供一个临时的上下文空间
        internal BiDictionary<object, (SerializedClass, int)> CopyIncludeObjects { get; } = new BiDictionary<object, (SerializedClass, int)>();

        /// <summary>
        /// 向数据空间中添加对象
        /// </summary>
        /// <param name="obj">要添加的对象</param>
        /// <param name="sc">该对象对应的序列化字段</param>
        /// <returns>
        /// <para><see langword="true"/>：添加成功</para>
        /// <see langword="false"/>：添加失败
        /// </returns>
        /// <remarks>！！慎用！！ 不正确添加对象可能造成错误的引用，一般情况下用户不应该使用该方法</remarks>
        public bool AddObject(object obj, SerializedClass sc)
        {
            //只有非字符串引用类型才会添加到数据空间中
            if (obj.GetType().IsClass && obj is not string)
            {
                if (!_IncludedObjects.Contains(obj))
                {
                    _IncludedObjects.AddOrUpdate(obj, sc);
                    return true;
                }
                else
                {
                    LogManager.Warn("SerializeSystem.SerializeDataSpace", $"要添加的对象已经存在于数据空间中", args: ("字段", sc.GetPath()));
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// 检查当前数据空间中是否存在某对象
        /// </summary>
        /// <param name="obj">要检查的对象</param>
        /// <param name="sc">如果对象存在，返回该对象关联的序列化字段，否则为null</param>
        /// <returns>
        /// <para><see langword="true"/>：存在</para>
        /// <see langword="false"/>：不存在
        /// </returns>
        public bool CheckObject(object obj, out SerializedClass sc)
        {
            sc = null;
            if (_IncludedObjects.Contains(obj))
            {
                sc = _IncludedObjects[obj];
                return true;
            }

            return false;
        }

        /// <summary>
        /// 检查当前数据空间是否存在某序列化字段
        /// </summary>
        /// <param name="sc">要检查的序列化字段</param>
        /// <param name="obj">如果序列化字段存在，返回其对应的对象，否则返回null</param>
        /// <returns>
        /// <para><see langword="true"/>：存在</para>
        /// <see langword="false"/>：不存在
        /// </returns>
        public bool CheckSerializedObject(SerializedClass sc, out object obj)
        {
            if (_IncludedObjects.Contains(sc))
            {
                obj = _IncludedObjects[sc];
                return true;
            }

            obj = null;
            return false;
        }

        /// <summary>
        /// 根据路径获取空间中的序列化字段
        /// </summary>
        /// <param name="path">路径字符串</param>
        /// <returns>如果存在返回对应序列化字段，否则返回null</returns>
        /// <remarks>如果相应的序列化字段还没有解析，则会首先将其解析</remarks>
        public SerializedClass GetSerializedObjectWithPath(string path, bool is_parse)
        {
            if (!string.IsNullOrEmpty(path))
            {
                string[] nodes = path.Split('/');
                if (nodes.Length >= 2)
                {
                    if (!_SDFs.Exists(x => x.Name == nodes[0]))
                    {
                        LogManager.Error("SerializeSystem", $"路径解析失败{path}（路径中的序列化文件不存在）");
                        return null;
                    }

                    SerializedDataFile sdf = _SDFs.First(x => x.Name == nodes[0]);
                    //SDF还没有解析
                    if (!sdf.IsParsed)
                    {
                        if (is_parse)
                            sdf.TryParse();
                        else
                            return null;
                    }

                    return sdf.GetSerializedObjectWithPath(nodes[1..]);
                }
                else
                {
                    LogManager.Error("SerializeSystem", $"路径解析失败{path}（节点数至少>=2）");
                    return null;
                }
            }
            else
            {
                LogManager.Error("SerializeSystem", $"路径为空或null");
                return null;
            }
        }

        /// <summary>
        /// 构造一个空的序列化文件
        /// </summary>
        /// <param name="name">文件名</param>
        /// <returns>构造的序列化文件</returns>
        public SerializedDataFile CreateSDF(string name)
        {
            return new SerializedDataFile(this, name);
        }

        /// <summary>
        /// 载入一个序列化文件
        /// </summary>
        /// <param name="name">文件名</param>
        /// <param name="dat">文件数据</param>
        /// <returns>如果载入成功返回对应的序列化文件，否则返回null</returns>
        public SerializedDataFile LoadSDF(string name, string dat)
        {
            LogManager.Assert(!string.IsNullOrEmpty(name), "SerializeDataSpace", "文件名（name）为空或null");
            LogManager.Assert(!string.IsNullOrEmpty(dat), "SerializeDataSpace", "文件内容（dat）为空或null");
            LogManager.Assert(!_SDFs.Exists(x => x.Name == name), "SerializeDataSpace", "相同文件名（{name}）的序列化文件已经存在");

            //清除注释内容
            //正则表达式：//.*\r\n
            // //....\r\n
            dat = Regex.Replace(dat, @"//.*\r\n", "");

            //删除所有空白字符
            char[] str_buf = new char[dat.Length + 1];
            bool is_in_str = false;
            int idx = 0;
            for (int i = 0; i < dat.Length; i++)
            {
                if (is_in_str)
                {
                    str_buf[idx++] = dat[i];
                    if (dat[i] == '"' && dat[i - 1] != '\\')
                        is_in_str = false;
                }
                else
                {
                    switch (dat[i])
                    {
                        case '\n':
                            break;
                        case '\r':
                            break;
                        case '\t':
                            break;
                        case ' ':
                            break;
                        case '"':
                            str_buf[idx++] = dat[i];
                            is_in_str = true;
                            break;
                        default:
                            str_buf[idx++] = dat[i];
                            break;
                    }
                }
            }

            dat = $"[#Root#]={{{new string(str_buf[0..idx])}}}";

            SerializedDataFile sdf = new SerializedDataFile(dat, this, name);

            _SDFs.Add(sdf);

            return sdf;
        }

        /// <summary>
        /// 获取序列化文件
        /// </summary>
        /// <param name="name">文件名</param>
        /// <returns>序列化文件</returns>
        public SerializedDataFile GetSDF(string name)
        {
            if (string.IsNullOrEmpty(name)) GameException.ArgInvalidThrow(nameof(name));

            SerializedDataFile sdf = _SDFs.FirstOrDefault(s => s.Name == name);
            if (sdf == null)
                LogManager.Error("SerializeSystem.SerializeDataSpace", $"没有找到对应的序列化文件（{name}）");

            return sdf;
        }

        internal void AddObjectToShelve(object obj, SerializedClass sc)
        {
            if (_ShelveObject0.Exists(x => x.Item1 == sc))
            {
                LogManager.Error("SerializeSystem", "序列化字段（sc）已经存在于_ShelveObject，不能重复添加");
                return;
            }

            if (_IncludedObjects.Contains(obj))
                LogManager.Warn("SerializeSystem", "对象（obj）已经存在于_IncludedObjects，不能重复添加");

            _ShelveObject0.Add((sc, obj));
        }

        /// <summary>
        /// 序列化后处理
        /// </summary>
        /// <remarks>主要处理序列化时搁置的引用（如果一个字段是对外部的引用，它就会被搁置）。必须也只能在序列化完全结束后调用</remarks>
        public void PostProcess()
        {
            //处理每一个搁置的引用
            foreach (var item in _ShelveObject0)
            {
                if (_IncludedObjects.TryGet(item.Item2, out SerializedClass sc))
                    item.Item1.RefSource = sc;
                else
                {
                    item.Item1.ObjectType |= SerializedObjectType.Error;
                    LogManager.Error("SerializeSystem", $"没有找到搁置对象对应的序列化字段");
                }
            }

            //清空搁置区
            _ShelveObject0.Clear();
        }
    }
}

