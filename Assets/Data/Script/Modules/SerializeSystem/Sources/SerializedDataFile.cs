using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Modules.LogSystem;


namespace Modules.SerializeSystem
{
    /// <summary>
    /// 序列化数据文件
    /// 所有序列化数据都被存入序列化数据文件中
    /// </summary>
    public class SerializedDataFile
    {
        //指示当前文件是否已经解析
        //当该成员为false时，说明RawData中包含着待解析内容
        public bool IsParsed { get; private set; }

        //待解析的数据
        //如果IsParsed是true，那么这里就为空
        //这里的数据是已经预处理过后的
        internal string RawData;

        /// <summary>
        /// 文件名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 该文件关联的数据空间
        /// </summary>
        public SerializeDataSpace Space { get; private set; }

        //引用链
        internal Stack<SerializedClass> SerializeChain { get; private set; } = new Stack<SerializedClass>();

        //Root中待添加的字段
        internal Queue<SerializedClass> FieldsWaitForAdd { get; private set; } = new Queue<SerializedClass>();

        /// <summary>
        /// 根字段
        /// </summary>
        public SerializedClass Root { get; private set; }


        /// <summary>
        /// 通过格式化数据生成序列化文件，该序列化文件已经加载<see cref="RawData"/>，但是还未解析
        /// </summary>
        /// <param name="raw">格式化数据</param>
        /// <param name="sds">关联的数据空间</param>
        /// <param name="name">文件名</param>
        public SerializedDataFile(string raw, SerializeDataSpace sds, string name)
        {
            RawData = raw;
            Space = sds;
            Name = name;
        }

        /// <summary>
        /// 构造一个空白的与特定数据空间相关联的序列化文件
        /// </summary>
        /// <param name="sds">关联的数据空间</param>
        /// <param name="name">文件名</param>
        public SerializedDataFile(SerializeDataSpace sds, string name)
        {
            Space = sds;
            Root = new SerializedClass(null, SerializedObjectType.Root | SerializedObjectType.Class, this);
            Root.SetName("#Root#");
            Name = name;
            IsParsed = true;

            SerializeChain.Push(Root);
        }

        /// <summary>
        /// 向序列化文件中直接写入要序列化的对象
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="name">对象名（也就是字段名）</param>
        /// <param name="cfg">序列化配置</param>
        /// <returns>序列化字段</returns>
        public SerializedClass WriteObject(object obj, string name, SerializeConfig cfg)
        {
            //为了保证顺序，首先序列化obj
            SerializedClass obj_sc = Root.AddField(name, obj, cfg, this);

            //最后添加序列化obj时产生的其他序列化对象
            while (FieldsWaitForAdd.Count != 0)
            {
                SerializedClass sc = FieldsWaitForAdd.Dequeue();
                Root.AddField(sc.Name, sc);
            }

            FieldsWaitForAdd.Clear();

            return obj_sc;
        }

        public T ReadObject<T>(string name, params object[] args)
        {
            if (IsParsed)
                return Root.UnSerializeField<T>(name, UnSerializeOption.None, args);
            else
            {
                LogManager.Error("SerializeSystem.SerializedDataFile", $"尝试从未解析的序列化文件（{Name}）中读取对象");
                return default;
            }
        }

        /// <summary>
        /// 根据路径节点获取序列化字段
        /// </summary>
        /// <param name="nodes">字段路径</param>
        /// <returns>序列化字段</returns>
        /// <remarks>
        /// 所谓路径节点就是字段路径里的各个子字段，如路径"xxx/yyy/zzz"对应的节点列表为{xxx,yyy,zzz}，
        /// <paramref name="nodes"/>中应不包含序列化文件自己
        /// </remarks>
        public SerializedClass GetSerializedObjectWithPath(string[] nodes)
        {
            SerializedClass sc = Root;
            foreach (string node in nodes)
            {
                if (!sc.Fields.TryGetValue(node, out SerializedClass _sc))
                {
                    LogManager.Error("SerializeSystem", $"字段路径错误（没有找到{sc.GetPath()}的子字段{node}）");
                    return null;
                }
                sc = _sc;
            }

            return sc;
        }

        /// <summary>
        /// 序列化开始（根据对象类型和配置参数，返回相应的序列化类）
        /// </summary>
        /// <param name="alias">别名</param>
        /// <param name="obj">要序列化的对象</param>
        /// <param name="cfg">序列化配置</param>
        /// <param name="sc_out">生成的序列化字段</param>
        /// <returns>
        /// <para><see langword="true"/>：该对象还没有被序列化，可以继续序列化操作</para>
        /// <see langword="flase"/>：该对象已经被序列化/序列化准备阶段出错，无法继续序列化操作
        /// </returns>
        /// <remarks>
        /// 检查对象在数据空间中是否已存在会在这一步完成。这个方法只应该在<see cref="IAllowSerialize"/>中的Serialize方法的开头使用
        /// </remarks>
        public bool SerializeBegin(string alias, object obj, SerializeConfig cfg, out SerializedClass sc_out)
        {
            //空对象
            if (obj == null)
            {
                sc_out = new SerializedClass(null, SerializedObjectType.Null, this);
                return false;
            }

            Type t = obj.GetType();
            //根据父对象的类型，判断子类型
            SerializedClass last_sc = SerializeChain.Peek();
            //额外的类型描述
            SerializedObjectType ex_type = SerializedObjectType.None;
            if (last_sc.ObjectType.HasFlag(SerializedObjectType.Enumerator))
                ex_type |= SerializedObjectType.Element;
            if (last_sc.ObjectType.HasFlag(SerializedObjectType.KeyValuePair))
            {
                if (last_sc.Fields.ContainsKey("#Key#"))
                    ex_type |= SerializedObjectType.DictionaryValue;
                else
                    ex_type |= SerializedObjectType.DictionaryKey;
            }

            if (t.IsValueType)
            {
                if (t.IsPrimitive || t.IsEnum)
                {
                    sc_out = new SerializedClass(null, SerializedObjectType.Error, cfg.FormatOption, this);
                    LogManager.Error("SerializeSystem", $"Primitive和Enum类型的对象永远不应该进入SerializeBegin方法",
                        args: new (string, object)[] { ("对象类型", t), ("SDF", Name) });
                    return false;
                }
                else if (typeof(IAllowSerialize).IsAssignableFrom(t))
                {
                    if (cfg.IsDelayDefine)
                    {
                        sc_out = new SerializedClass(alias, SerializedObjectType.Reference, cfg.FormatOption, this);

                        //延迟定义时要将Root入栈
                        //这样其父对象就是Root
                        SerializeChain.Push(Root);

                        cfg.ConfigOption &= ~SerializeConfigOption.DelayDefine;
                        GeneralSerializeMethod gsm = SerializeTool.GetGeneralSerializeMethod(t);
                        sc_out.RefSource = gsm(obj, null, cfg, this);
                        sc_out.RefSource.SetName(alias ?? Space.NextAlias);
                        FieldsWaitForAdd.Enqueue(sc_out.RefSource);

                        SerializeChain.Pop();

                        return false;
                    }
                    else
                    {
                        sc_out = new SerializedClass(null, SerializedObjectType.Struct, cfg.FormatOption, this);
                        SerializeChain.Push(sc_out);
                    }
                }
                else if (typeof(IEnumerable).IsAssignableFrom(t))
                {
                    //获取元素的类型
                    /*SerializedObjectType ele_type;
                    if (cfg.IsElementDelayDefine)
                        ele_type = SerializedObjectType.Reference;
                    else if (t.IsGenericType)
                    {
                        Type gen_t = t.GetGenericArguments()[0];
                        if (gen_t.IsGenericType && gen_t.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                            ele_type = SerializedObjectType.KeyValuePair;
                        else if (typeof(IEnumerable).IsAssignableFrom(gen_t))
                            ele_type = SerializedObjectType.Enumerator;
                        else if (typeof(IAllowSerialize).IsAssignableFrom(gen_t))
                        {
                            if (gen_t.IsValueType)
                                ele_type = SerializedObjectType.Struct;
                            else
                                ele_type = SerializedObjectType.Class;
                        }
                        else
                            ele_type = SerializedObjectType.Basic;
                    }
                    else
                        ele_type = SerializedObjectType.None;*/

                    if (cfg.IsDelayDefine)
                    {
                        sc_out = new SerializedClass(alias, SerializedObjectType.Reference, cfg.FormatOption, this);

                        SerializeChain.Push(Root);

                        cfg.ConfigOption &= ~SerializeConfigOption.DelayDefine;
                        GeneralSerializeMethod gsm = SerializeTool.GetGeneralSerializeMethod(t);
                        sc_out.RefSource = gsm(obj, null, cfg, this);
                        FieldsWaitForAdd.Enqueue(sc_out.RefSource);
                        sc_out.RefSource.SetName(alias ?? Space.NextAlias);
                        //sc_out.RefSource.ElementType = ele_type;

                        SerializeChain.Pop();

                        return false;
                    }
                    else
                    {
                        sc_out = new SerializedClass(null, SerializedObjectType.Struct | SerializedObjectType.Enumerator, cfg.FormatOption, this);
                        //sc_out.ElementType = ele_type;
                        SerializeChain.Push(sc_out);
                    }
                }
                else if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                {
                    sc_out = new SerializedClass(null, SerializedObjectType.KeyValuePair, cfg.FormatOption, this);
                    SerializeChain.Push(sc_out);
                }
                else if (typeof(ITuple).IsAssignableFrom(t))
                {
                    if (cfg.IsDelayDefine)
                    {
                        sc_out = new SerializedClass(alias, SerializedObjectType.Reference, cfg.FormatOption, this);

                        SerializeChain.Push(Root);

                        cfg.ConfigOption &= ~SerializeConfigOption.DelayDefine;
                        GeneralSerializeMethod gsm = SerializeTool.GetGeneralSerializeMethod(t);
                        sc_out.RefSource = gsm(obj, null, cfg, this);
                        sc_out.RefSource.SetName(alias ?? Space.NextAlias);
                        FieldsWaitForAdd.Enqueue(sc_out.RefSource);

                        SerializeChain.Pop();

                        return false;
                    }
                    else
                    {
                        sc_out = new SerializedClass(null, SerializedObjectType.Tuple, cfg.FormatOption, this);
                        SerializeChain.Push(sc_out);
                    }
                }
                else
                {
                    sc_out = new SerializedClass(null, SerializedObjectType.Error, this);
                    LogManager.Error("SerializeSystem", $"对象的类型（{t}）不是可序列化的（既不是内置可序列化类也没有实现IAllowSerialize接口）",
                                        args: new (string, object)[] { ("序列化文件", Name), ("父字段", SerializeChain.Peek()?.GetPath()) });
                    return false;
                }
            }
            else
            {
                //首先检查是否在数据空间中已经存在
                if (Space.CheckObject(obj, out SerializedClass temp))
                {
                    sc_out = new SerializedClass(null, SerializedObjectType.Reference | ex_type, cfg.FormatOption, this);
                    sc_out.RefSource = temp;

                    return false;
                }
                else
                {
                    //这意味着这个字段是从基类那里序列化得到的
                    if (typeof(SerializedClass).IsAssignableFrom(t))
                    {
                        sc_out = obj as SerializedClass;

                        if (sc_out.ObjectType.HasFlag(SerializedObjectType.Reference) ||
                            sc_out.ObjectType.HasFlag(SerializedObjectType.Error) ||
                            sc_out.ObjectType.HasFlag(SerializedObjectType.Null))
                            return false;

                        if (sc_out.ObjectType.HasFlag(SerializedObjectType.Class) || sc_out.ObjectType.HasFlag(SerializedObjectType.Struct))
                            //这里通过将父字段压入栈，来重构上下文
                            SerializeChain.Push(sc_out.Parent);
                        else
                            LogManager.Error("SerializeSystem", $"对于一个合法的序列化类，其类型不能是{sc_out.ObjectType}，@{sc_out.GetPath()}");

                        return true;
                    }
                    else if (typeof(IAllowSerialize).IsAssignableFrom(t))
                    {
                        //检查是否为延迟定义
                        if (cfg.IsDelayDefine)
                        {
                            sc_out = new SerializedClass(null, SerializedObjectType.Reference | ex_type, cfg.FormatOption, this);

                            SerializeChain.Push(Root);
                            cfg.ConfigOption &= ~SerializeConfigOption.DelayDefine;
                            GeneralSerializeMethod gsm = SerializeTool.GetGeneralSerializeMethod(t);
                            sc_out.RefSource = gsm(obj, null, cfg, this);
                            sc_out.RefSource.SetName(alias ?? Space.NextAlias);
                            FieldsWaitForAdd.Enqueue(sc_out.RefSource);
                            SerializeChain.Pop();

                            Space.AddObject(obj, sc_out.RefSource);

                            return false;
                        }
                        else if (cfg.IsExtern)
                        {
                            sc_out = new SerializedClass(null, SerializedObjectType.Reference | ex_type, cfg.FormatOption, this);

                            //检查引用是否已经被记录
                            if (Space.CheckObject(obj, out SerializedClass refed_sc))
                                sc_out.RefSource = refed_sc;
                            else
                                Space.AddObjectToShelve(obj, sc_out);
                        }
                        else
                        {
                            sc_out = new SerializedClass(null, SerializedObjectType.Class | ex_type, cfg.FormatOption, this);
                            Space.AddObject(obj, sc_out);
                            SerializeChain.Push(sc_out);
                        }
                    }
                    //枚举类型，包含了数组、列表、字典等等继承IEnumerable的类
                    else if (typeof(IEnumerable).IsAssignableFrom(t))
                    {
                        //TODO:获取元素的类型
                        //获取元素的类型
                        /*SerializedObjectType ele_type;

                        if (cfg.IsElementDelayDefine)
                            ele_type = SerializedObjectType.Reference;
                        else if (t.IsGenericType)
                        {
                            Type gen_t = t.GetGenericArguments()[0];
                            if (gen_t.IsGenericType && gen_t.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                                ele_type = SerializedObjectType.KeyValuePair;
                            else if (typeof(IEnumerable).IsAssignableFrom(gen_t))
                                ele_type = SerializedObjectType.Enumerator;
                            else if (typeof(IAllowSerialize).IsAssignableFrom(gen_t))
                            {
                                if (gen_t.IsValueType)
                                    ele_type = SerializedObjectType.Struct;
                                else
                                    ele_type = SerializedObjectType.Class;
                            }
                            else
                                ele_type = SerializedObjectType.Basic;
                        }
                        else
                            ele_type = SerializedObjectType.None;*/

                        //是否为延迟定义
                        if (cfg.IsDelayDefine)
                        {
                            //延迟定义时，sc_out只是一个引用，它指向sc_out.RefSource（实际字段）
                            //而sc_out.RefSource则是Root的直接子字段
                            sc_out = new SerializedClass(alias, SerializedObjectType.Reference | ex_type, cfg.FormatOption, this);

                            SerializeChain.Push(Root);

                            //实际字段要取消延迟定义选项
                            cfg.ConfigOption &= ~SerializeConfigOption.DelayDefine;
                            GeneralSerializeMethod gsm = SerializeTool.GetGeneralSerializeMethod(t);
                            sc_out.RefSource = gsm(obj, null, cfg, this);
                            //实际字段加入等待添加的队列，确保它排在Root后
                            FieldsWaitForAdd.Enqueue(sc_out.RefSource);
                            sc_out.RefSource.SetName(alias ?? Space.NextAlias);
                            //sc_out.RefSource.ElementType = ele_type;

                            SerializeChain.Pop();

                            //延迟定义时，对象在Root字段中序列化时已经添加到了空间中，这里不用重复添加
                            //Space.AddObject(obj, sc_out.RefSource);

                            return false;
                        }
                        //是否为外部引用
                        else if (cfg.IsExtern)
                        {
                            sc_out = new SerializedClass(null, SerializedObjectType.Reference | ex_type, cfg.FormatOption, this);

                            //检查引用是否已经被记录
                            if (Space.CheckObject(obj, out SerializedClass refed_sc))
                                sc_out.RefSource = refed_sc;
                            else
                                Space.AddObjectToShelve(obj, sc_out);

                            return false;
                        }
                        else
                        {
                            sc_out = new SerializedClass(null, SerializedObjectType.Enumerator | SerializedObjectType.Class | ex_type, cfg.FormatOption, this);
                            //sc_out.ElementType = ele_type;
                            Space.AddObject(obj, sc_out);
                            SerializeChain.Push(sc_out);
                        }
                    }
                    else
                    {
                        sc_out = new SerializedClass(null, SerializedObjectType.Error | ex_type, this);
                        LogManager.Error("SerializeSystem", $"对象的类型（{t}）不可序列化（既不是内置可序列化类也没有实现IAllowSerialize接口）",
                                          args: new (string, object)[] { ("序列化文件", Name), ("父字段", SerializeChain.Peek()?.GetPath()) });
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 序列化结束
        /// </summary>
        /// <param name="sc">序列化字段</param>
        /// <returns><paramref name="sc"/>自身</returns>
        /// <remarks>
        /// 该方法完成序列化过程的后续工作。这个方法只应该在<see cref="IAllowSerialize"/>中的Serialize方法的结尾使用
        /// </remarks>
        public SerializedClass SerializeEnd(SerializedClass sc)
        {
            if (sc.ObjectType.HasFlag(SerializedObjectType.Reference) ||
                sc.ObjectType.HasFlag(SerializedObjectType.Error) ||
                sc.ObjectType.HasFlag(SerializedObjectType.Null))
                return sc;

            if (sc.ObjectType.HasFlag(SerializedObjectType.Class) || sc.ObjectType.HasFlag(SerializedObjectType.Struct) || sc.ObjectType.HasFlag(SerializedObjectType.Tuple))
                //这里会顺便设置其父字段（在存在继承关系的序列化类的序列化中，提前设置父字段，使其在子类的序列化过程中可以重建上下文）
                sc.SetParent(SerializeChain.Pop());
            else
                LogManager.Warn("SerializeSystem", $"序列化类型错误，类型不能是{sc.ObjectType}，{sc.GetPath()}");

            return sc;
        }

        /// <summary>
        /// 生成文件字符串内容
        /// </summary>
        /// <returns>格式化了的字符串，可以直接写入到文件中</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(1024);

            sb.AppendLine("//This file was generated by program automatically.");
            sb.AppendLine($"//File : {Name}");
            sb.AppendLine($"//Date : {DateTime.Now}\r\n");

            foreach (SerializedClass sc_temp in Root.Fields.Values)
                sb.AppendLine(sc_temp.ToString($"0,false")).Append("\r\n");

            return sb.ToString();
        }

        /// <summary>
        /// 解析该序列化文件（要确保序列化字符串已经载入）
        /// </summary>
        /// <returns>
        /// <para><see langword="true"/>：解析成功</para>
        /// <see langword="false"/>：解析失败
        /// </returns>
        public bool TryParse()
        {
            if (string.IsNullOrEmpty(RawData))
            {
                LogManager.Warn("SerializeSystem", $"解析序列化文件{Name}失败，因为RawData还没有载入");
                return false;
            }

            Root = SerializedClass.Parse(RawData, 9, this, out _);

            if (Root.ObjectType.HasFlag(SerializedObjectType.Error))
                LogManager.Warn("SerializeSystem", $"解析序列化文件{Name}时，#Root#为Error类型");

            Root.SetName("#Root#");
            SerializeChain.Clear();
            IsParsed = true;

            while (FieldsWaitForAdd.Count != 0)
            {
                SerializedClass sc = FieldsWaitForAdd.Dequeue();
                sc.RefSource ??= Space.GetSerializedObjectWithPath(sc.Alias, true) ?? new SerializedClass(null, SerializedObjectType.Error, this);
                sc.GetRefrenceSource().RefCount++;
            }

            FieldsWaitForAdd.Clear();

            return true;
        }
    }
}
