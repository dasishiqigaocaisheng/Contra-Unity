using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Modules.LogSystem;

namespace Modules.SerializeSystem
{
    /// <summary>
    /// 已序列化类
    /// 所有可序列化类经过序列化后，都会转化为已序列化类
    /// </summary>
    public class SerializedClass : IFormattable
    {
        /// <summary>
        /// 该字段的名字
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 该字段的别名
        /// </summary>
        public string Alias { get; private set; }

        /// <summary>
        /// 该字段的特性
        /// </summary>
        internal List<SerializeAttributeBase> Attributes { get; set; }

        /// <summary>
        /// 序列化类型 不同的类型序列化后可能会有不同的序列化类型
        /// </summary>
        public SerializedObjectType ObjectType { get; set; }

        /// <summary>
        /// 字段是否是根字段
        /// </summary>
        public bool IsRoot => Name == "#Root#";

        //TODO:该属性的判断相关的代码还未编写完整
        /// <summary>
        /// 元素类型
        /// </summary>
        /// <remarks>当该序列化类是<see cref="SerializedObjectType.Enumerator"/>类型时，该参数代表其元素的类型</remarks> 
        [Obsolete("该属性相关的代码还未编写完整，在此之前不要使用它")]
        public SerializedObjectType ElementType { get; set; }

        /// <summary>
        /// 格式化选项
        /// </summary>
        public SerializeFormatOption FormatOption { get; private set; }

        /// <summary>
        /// 子字段
        /// </summary>
        public Dictionary<string, SerializedClass> Fields { get; private set; }

        /// <summary>
        /// 子字段个数
        /// </summary>
        public int FieldCount => Fields == null ? 0 : Fields.Count;

        /// <summary>
        /// 对子字段按字段名索引
        /// </summary>
        /// <param name="name">字段名</param>
        /// <returns>子字段</returns>
        /// <remarks>注意：当<paramref name="name"/>对应的字段不存在时，该方法返回<see cref="SerializedObjectType.Error"/>类型的序列化字段</remarks>
        public SerializedClass this[string name]
        {
            get
            {
                if (Fields != null && Fields.ContainsKey(name))
                    return Fields[name];
                else
                    return new SerializedClass(null, SerializedObjectType.Error, SDF);
            }
        }

        /// <summary>
        /// 对子字段按编号索引
        /// </summary>
        /// <remarks>只能在Array等类型中使用</remarks>
        /// <param name="index">字段索引</param>
        /// <returns>子字段</returns>
        public SerializedClass this[int index] { get => Fields[$"#{index}#"]; }

        /// <summary>
        /// 父序列化类
        /// </summary>
        public SerializedClass Parent { get; private set; }

        /// <summary>
        /// 引用源
        /// </summary>
        /// <remarks>如果该序列化类是引用类型，则该值指向它的引用</remarks>
        public SerializedClass RefSource { get; set; }

        /// <summary>
        /// 从属的序列化文件
        /// </summary>
        public SerializedDataFile SDF { get; private set; }

        /// <summary>
        /// 内部标记字符串的最大长度，也就是#xxx#中xxx的最大长度
        /// </summary>
        /// <remarks>如果解析时，内部标记超出了这个长度，则会产生异常</remarks>
        private const int _InternalSignLen = 10;

        /// <summary>
        /// 派生字段
        /// </summary>
        internal SerializedClass DeriveField { get; set; }

        /// <summary>
        /// 引用计数
        /// </summary>
        internal int RefCount { get; set; }

        /// <summary>
        /// 字段每被复制一次，该属性+1；每结束复制一次，该属性-1
        /// </summary>
        internal int CopyCount { get; set; }

        #region Constructor

        //通用构造方法
        public SerializedClass(string alias, SerializedObjectType type, SerializeFormatOption fmt_opt, SerializedDataFile sdf)
        {
            Alias = alias;
            ObjectType = type;
            FormatOption = fmt_opt;
            SDF = sdf;

            //只有非基本类型才会有子字段
            if (!(type.HasFlag(SerializedObjectType.Basic) || type.HasFlag(SerializedObjectType.Error)))
                Fields = new Dictionary<string, SerializedClass>();
        }

        public SerializedClass(string alias, SerializedObjectType type, SerializedDataFile sdf) : this(alias, type, SerializeFormatOption.Auto, sdf) { }

        #endregion

        /// <summary>
        /// 添加子字段
        /// </summary>
        /// <param name="name">字段名</param>
        /// <param name="obj">实例</param>
        /// <param name="alias">别名</param>
        /// <param name="cfg">序列化配置</param>
        /// <param name="sdf">对应的序列化数据文件</param>
        /// <returns>由obj生成的已序列化类</returns>
        public SerializedClass AddField(string name, object obj, string alias, SerializeConfig cfg, SerializedDataFile sdf)
        {
            if (Fields.ContainsKey(name))
            {
                LogManager.Warn("SerializeSystem", $"要添加的字段名：{name}重复");
                return null;
            }

            GeneralSerializeMethod gsm = SerializeTool.GetGeneralSerializeMethod(obj?.GetType());
            SerializedClass sc = gsm(obj, alias, cfg, sdf);
            sc.Parent = this;
            sc.SetName(name);
            Fields.Add(name, sc);

            return sc;
        }

        /// <summary>
        /// 添加子字段
        /// </summary>
        /// <remarks>省略了alias</remarks>
        /// <param name="name">字段名</param>
        /// <param name="obj">实例</param>
        /// <param name="cfg">序列化配置</param>
        /// <param name="sdf">对应的序列化数据文件</param>
        /// <returns>由obj生成的已序列化类</returns>
        public SerializedClass AddField(string name, object obj, SerializeConfig cfg, SerializedDataFile sdf)
        {
            return AddField(name, obj, null, cfg, sdf);
        }

        /// <summary>
        /// 添加子字段
        /// </summary>
        /// <remarks>省略了alias，且使用默认配置</remarks>
        /// <param name="name">字段名</param>
        /// <param name="obj">实例</param>
        /// <param name="sdf">对应的序列化数据文件</param>
        /// <returns>由obj生成的已序列化类</returns>
        public SerializedClass AddField(string name, object obj, SerializedDataFile sdf)
        {
            return AddField(name, obj, null, SerializeConfig.Default, sdf);
        }

        /// <summary>
        /// 根据已经给出的序列化类添加子字段
        /// </summary>
        /// <remarks>省略了alias，且使用默认配置</remarks>
        /// <param name="name">字段名</param>
        /// <param name="sc">给定的序列化类</param>
        /// <returns><paramref name="sc"/></returns>
        public SerializedClass AddField(string name, SerializedClass sc)
        {
            return AddField(name, sc, SerializeConfig.Default, sc.SDF);
        }

        /// <summary>
        /// 判断是否包含特定名称的字段
        /// </summary>
        /// <param name="name">字段名称</param>
        /// <param name="sc">如果存在，则这个参数存储了名称对应的字段</param>
        /// <returns>是否包含</returns>
        /// <remarks>这里不考虑派生的情况</remarks>
        public bool HasField(string name, out SerializedClass sc)
        {
            return Fields.TryGetValue(name, out sc);
        }

        /// <summary>
        /// 设置序列化类的Name属性
        /// </summary>
        /// <param name="name">要设置的值</param>
        /// <remarks>!!慎用!! 强制改变字段名可能使得名字和它在父字段中记录的值不一致</remarks>
        internal void SetName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError("");
                return;
            }

            Name = name;
        }

        /// <summary>
        /// 设置父序列化类
        /// </summary>
        /// <param name="sc">父序列化类</param>
        internal void SetParent(SerializedClass sc)
        {
            Parent = sc;
        }

        /// <summary>
        /// 获取该类的路径
        /// </summary>
        /// <param name="ref_sdf">参考的序列化文件</param>
        /// <returns>路径字符串</returns>
        /// <remarks>
        /// <para>路径格式为：&lt;SDF.Name&gt;/xxx/xxx/&lt;this.Name&gt;</para>
        /// 如果<see cref="SDF"/>==<paramref name="ref_sdf"/>，那么格式为：./xxx/xxx/&lt;this.Name&gt;，也就是SDF名字会替换为'.'
        /// </remarks>
        public string GetPath(SerializedDataFile ref_sdf = null)
        {
            string path = "";
            SerializedClass sc = this;
            while (sc != null)
            {
                if (sc.IsRoot)
                {
                    if (ref_sdf != null && ref_sdf == SDF)
                        path = "./" + path;
                    else
                        path = SDF.Name + "/" + path;
                }
                else
                    path = sc.Name + "/" + path;
                sc = sc.Parent;
            }
            return path;
        }

        /// <summary>
        /// 获取最终的引用源
        /// </summary>
        /// <returns>最终的引用源</returns>
        /// <remarks>
        /// 该方法会沿着RefSource一直检查，直到第一个没有<see cref="SerializedObjectType.Reference"/>的字段
        /// 如果字段不是引用类型的，那么该方法返回字段自己
        /// </remarks>
        public SerializedClass GetRefrenceSource()
        {
            SerializedClass sc = this;
            while (sc.ObjectType.HasFlag(SerializedObjectType.Reference))
            {
                if (sc.RefSource == null)
                    return null;
                sc = sc.RefSource;
            }
            return sc;
        }

        /// <summary>
        /// 检查字段是否具有特性
        /// </summary>
        /// <typeparam name="T">特性类型</typeparam>
        /// <param name="satb">如果存在，这个参数返回找到的特性</param>
        /// <returns>是否存在</returns>
        internal bool HasAttribute<T>(out T satb) where T : SerializeAttributeBase
        {
            satb = null;
            if (Attributes == null)
                return false;
            else
            {
                satb = Attributes.FirstOrDefault(x => x is T) as T;
                return satb != null;
            }
        }

        /// <summary>
        /// 根据条件寻找第一个符合要求的子字段
        /// </summary>
        /// <param name="cond">搜索条件</param>
        /// <returns>
        /// 找到的字段
        /// 如果没有找到任何符合条件的字段，则返回<see cref="SerializedObjectType.Error"/>类型的空字段
        /// </returns>
        public SerializedClass FirstWith(Func<SerializedClass, bool> cond)
        {
            return Fields?.FirstOrDefault(x => cond(x.Value)).Value ?? new SerializedClass(null, SerializedObjectType.Error, SDF);
        }

        // TODO:此方法错误处理部分有待完善
        /// <summary>
        /// 从字符串解析出序列化类
        /// </summary>
        /// <param name="str">待解析的字符串</param>
        /// <param name="idx">str开始处的偏移，例如：当idx==1，则从str[1]开始解析</param>
        /// <param name="sdf">该序列化类从属的序列化文件</param>
        /// <param name="idx_o">解析过的字符串的最后一个字符的索引+1，有效的字符串可能只是str的一部分，该值则给出了这个有效字符串的结尾索引</param>
        /// <returns>解析出来的序列化类</returns>
        /// <remarks>
        /// <para>
        /// 1.该函数只会解析它遇见的第一个可解析字符串，即使<paramref name="str"/>中包含多个可解析字符串
        /// </para>
        /// <para>
        ///  2.给出的<paramref name="str"/>应该是不包含字段名的
        ///  例如下面的字符串是有效的：
        ///           <c><paramref name="str"/>="{[xxx]=1[yyy]=2}"</c>，该字符串解析出的序列化类将会包含两个子字段
        ///  而下面的的字符串是无效的：
        ///           <paramref name="str"/>="[nnn]{[xxx]=1[yyy]=2}"，因为它包含了字段名"nnn"；然而，也可以同时给出偏移量<paramref name="idx"/>=5，将字段名部分跳过
        /// </para>
        /// <para>
        /// 3.对于上面两个例子，输出的<paramref name="idx_o"/>都将等于str的结尾索引的后一个索引，也就是<paramref name="str"/>的长度值（这个索引超出<paramref name="str"/>索引范围）
        ///       而对于下面的例子，<paramref name="idx_o"/>将会指向<paramref name="str"/>的最后一个字符的索引
        ///   <paramref name="str"/> = "{[xxx]=1[yyy]=2}{"，因为<paramref name="str"/>[0..16]（即"{[xxx]=1[yyy]=2}"） 是一个完整的可解析字符串，函数解析完这部分字符后就会返回
        /// </para>
        /// </remarks>
        public static SerializedClass Parse(string str, int idx, SerializedDataFile sdf, out int idx_o)
        {
            try
            {
                bool is_in_str = false;
                int state = 0, stack = 0;
                int idx_save0 = 0, idx_save1 = 0;

                idx_o = idx;
                SerializedObjectType t = SerializedObjectType.None;
                List<SerializeAttributeBase> atbs = null;
                for (int i = idx; i < str.Length; i++)
                {
                    switch (state)
                    {
                        /*
                         * 类型初步分类
                         * 1.以'{'开头：
                         *  a.普通Class类型：{[xx]=xxx[yy]=yyy...}
                         *  b.Enumerator类型：{xx,xx,xx,...}
                         *  c.引用类型
                         *  d.空置类型
                         * 2.以'"'开头：必定是String类型
                         * 3.以'#'开头：特殊类型
                         * 4.以'('开头：Tuple或者Vector类型
                         * 5.其他字符：Basic/Enum类型
                         * 6.特性：<xxxx>
                        */
                        case 0:
                            switch (str[i])
                            {
                                case '{':
                                    idx_save0 = i + 1;
                                    state = 1;
                                    break;
                                case '"':
                                    idx_save0 = i + 1;
                                    t = SerializedObjectType.String;
                                    goto LABEL0;
                                case '#':
                                    idx_save0 = i + 1;
                                    state = 2;
                                    break;
                                case '<':
                                    idx_save0 = i + 1;
                                    state = 3;
                                    break;
                                case '(':
                                    idx_save0 = i + 1;
                                    state = 4;
                                    break;
                                default:
                                    idx_save0 = i;
                                    if ((str[i] >= 'a' && str[i] <= 'z') || (str[i] >= 'A' && str[i] <= 'Z') || str[i] == '_')
                                    {
                                        if (str[i..(i + 4)] == "true" &&
                                            !((str[i + 4] >= 'a' && str[i + 4] <= 'z') || (str[i + 4] >= 'A' && str[i + 4] <= 'Z') || str[i + 4] == '_'))
                                        {
                                            t = SerializedObjectType.Basic;
                                            goto LABEL0;
                                        }
                                        else if (str[i..(i + 5)] == "false" &&
                                            !((str[i + 5] >= 'a' && str[i + 5] <= 'z') || (str[i + 5] >= 'A' && str[i + 5] <= 'Z') || str[i + 5] == '_'))
                                        {
                                            t = SerializedObjectType.Basic;
                                            goto LABEL0;
                                        }
                                        t = SerializedObjectType.Enum;
                                    }
                                    else
                                        t = SerializedObjectType.Basic;
                                    goto LABEL0;
                            }
                            break;
                        /*
                         * '{'开头的类型细分：
                         *  1.普通Class类型：{[xx]=xxx,[yy]=yyy,...}
                         *  2.Enumerator类型：{xx,xx,xx,...}
                         *  3.Keyvaluepair类型：{xx:yy}
                         *  4.引用类型
                         *  5.空置类型：{}
                         */
                        case 1:
                            switch (str[i])
                            {
                                case '"':
                                    if (str[i - 1] != '\\')
                                        is_in_str = !is_in_str;
                                    break;
                                case '/':
                                    if (!is_in_str && stack == 0)
                                    {
                                        t = SerializedObjectType.Reference;
                                        goto LABEL0;
                                    }
                                    break;
                                case '{':
                                    if (!is_in_str)
                                        stack++;
                                    break;
                                case ',':
                                    if (!is_in_str && stack == 0)
                                    {
                                        t = SerializedObjectType.Enumerator;
                                        goto LABEL0;
                                    }
                                    break;
                                case '[':
                                    if (!is_in_str && stack == 0)
                                    {
                                        t = SerializedObjectType.Class;
                                        goto LABEL0;
                                    }
                                    break;
                                case ':':
                                    if (!is_in_str && stack == 0)
                                    {
                                        //寄存Value的起始字符索引
                                        idx_save1 = i + 1;
                                        t = SerializedObjectType.KeyValuePair;
                                        goto LABEL0;
                                    }
                                    break;
                                case '}':
                                    if (!is_in_str)
                                    {
                                        if (i == idx_save0)
                                        {
                                            t = SerializedObjectType.Empty;
                                            goto LABEL0;
                                        }

                                        if (stack == 0)
                                        {
                                            t = SerializedObjectType.Enumerator;
                                            goto LABEL0;
                                        }
                                        else
                                            stack--;
                                    }
                                    break;
                                case '(':
                                    if (!is_in_str)
                                        stack++;
                                    break;
                                case ')':
                                    if (!is_in_str)
                                        stack--;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        /*
                         * '#'开头的类型细分：
                         * 这种情况只可能是一些内置的标记
                         *  1.#Null#
                         *  2.#Error#
                         */
                        case 2:
                            if (str[i] == '#')
                            {
                                idx_save1 = i - 1;
                                if (str[idx_save0..i] == "Null")
                                    t = SerializedObjectType.Null;
                                else if (str[idx_save0..i] == "Error")
                                    t = SerializedObjectType.Error;
                                else
                                {
                                    //未知的内置标记
                                    t = SerializedObjectType.Error;
                                    LogManager.Warn("SerializeSystem", $"未识别的内部标记：{str[idx_save0..i]}");
                                }
                                goto LABEL0;
                            }
                            else if (i - idx_save0 + 1 == _InternalSignLen)
                            {
                                //标记长度超范围
                                LogManager.Warn("SerializeSystem", $"未识别的内部标记（标记过长）");
                            }
                            break;
                        /*
                         * 获取特性内容
                         */
                        case 3:
                            if (str[i] == '>')
                            {
                                atbs ??= new List<SerializeAttributeBase>();
                                atbs.Add(SerializeAttributeBase.Parse(str[idx_save0..i]));
                                state = 0;
                            }
                            break;
                        /*
                         * 以'('开头，有两种可能
                         * 1.Tuple类型：(xxx;xxx;xxx)
                         * 2.Vector类型：(x,y,z)
                         */
                        case 4:
                            if (str[i] == '"' && str[i - 1] != '\\')
                                is_in_str = !is_in_str;
                            else if (!is_in_str)
                            {
                                if (str[i] == ',' && stack == 0)
                                {
                                    idx_save0--;
                                    t = SerializedObjectType.Basic;
                                    goto LABEL0;
                                }
                                else if (str[i] == '{' || str[i] == '(')
                                    stack++;
                                else if (str[i] == '}')
                                    stack--;
                                else if (str[i] == ')')
                                {
                                    if (stack == 0)
                                    {
                                        t = SerializedObjectType.Tuple;
                                        goto LABEL0;
                                    }
                                    else
                                        stack--;
                                }
                                else if (str[i] == ';' && stack == 0)
                                {
                                    t = SerializedObjectType.Tuple;
                                    goto LABEL0;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }

            //类型判断结束
            LABEL0:
                SerializedClass parent = sdf.SerializeChain.Count != 0 ? sdf.SerializeChain.Peek() : null;
                SerializedClass sc = null;
                state = 0;
                //根据类型解析内容
                switch (t)
                {
                    case SerializedObjectType.Class:
                        sc = new SerializedClass(null, t, sdf);
                        List<SerializeAttributeBase> field_atbs = null;
                        sdf.SerializeChain.Push(sc);
                        for (int i = idx_save0; i < str.Length; i++)
                        {
                            switch (state)
                            {
                                //搜寻字段名开头标记：'['或特性开头'<'
                                case 0:
                                    if (str[i] == '[')
                                    {
                                        idx_save0 = i + 1;
                                        state = 1;
                                    }
                                    else if (str[i] == '<')
                                    {
                                        idx_save0 = i + 1;
                                        state = 3;
                                    }
                                    else if (str[i] == '}')
                                    {
                                        idx_o = i + 1;
                                        sdf.SerializeChain.Pop();
                                        goto LABEL1;
                                    }
                                    break;
                                //搜寻字段名结束标记：]
                                case 1:
                                    if (str[i] == ']')
                                    {
                                        idx_save1 = i;
                                        state = 2;
                                    }
                                    break;
                                //解析字段
                                case 2:
                                    if (str[i] == '=')
                                    {
                                        string name = str[idx_save0..idx_save1];
                                        if (!string.IsNullOrEmpty(name))
                                        {
                                            SerializedClass temp = Parse(str, i + 1, sdf, out i);
                                            if (temp != null)
                                            {
                                                sc.AddField(name, temp).Attributes = field_atbs;
                                                field_atbs = null;
                                                i--;
                                                state = 0;
                                            }
                                            //字段内容解析失败
                                            else
                                            {
                                                state = 0;
                                                Debug.LogError("");
                                            }
                                        }
                                        //字段名解析失败
                                        else
                                        {
                                            state = 0;
                                            Debug.LogError("");
                                        }
                                    }
                                    else
                                    {
                                        state = 0;
                                        Debug.LogError("");
                                    }
                                    break;
                                case 3:
                                    if (str[i] == '>')
                                    {
                                        field_atbs ??= new List<SerializeAttributeBase>();
                                        field_atbs.Add(SerializeAttributeBase.Parse(str[idx_save0..i]));
                                        state = 0;
                                    }
                                    break;
                            }
                        }
                        sdf.SerializeChain.Pop();
                        Debug.LogError("");
                        break;
                    case SerializedObjectType.String:
                        //搜寻字符串结束标注："
                        for (int i = idx_save0; i < str.Length; i++)
                        {
                            if (str[i] == '"' && str[i - 1] != '\\')
                            {
                                sc = new SerializedClass(str[(idx_save0 - 1)..(i + 1)], t, sdf);
                                idx_o = i + 1;
                                goto LABEL1;
                            }
                        }
                        Debug.LogError("");
                        break;
                    case SerializedObjectType.Enumerator:
                        {
                            sc = new SerializedClass(null, t, sdf);
                            sdf.SerializeChain.Push(sc);

                            //有零个元素的情况
                            if (str[idx_save0] == '}')
                            {
                                sc.AddField("#Count#", 0, sdf);
                                idx_o = idx_save0 + 1;
                                sdf.SerializeChain.Pop();
                                goto LABEL1;
                            }

                            SerializedClass ele = Parse(str, idx_save0, sdf, out int i);
                            if (ele != null)
                                sc.AddField("#0#", ele);
                            else
                            {
                                sc.AddField("#0#", new SerializedClass(null, SerializedObjectType.Error, sdf));
                                Debug.LogError("");
                            }

                            int ele_idx = 1;
                            for (; i < str.Length; i++)
                            {
                                if (str[i] == ',')
                                {
                                    ele = Parse(str, i + 1, sdf, out i);
                                    if (ele != null)
                                    {
                                        sc.AddField($"#{ele_idx++}#", ele);
                                        i--;
                                    }
                                    else
                                    {
                                        sc.AddField($"#{ele_idx++}#", new SerializedClass(null, SerializedObjectType.Error, sdf));
                                        Debug.LogError("");
                                    }
                                }
                                else if (str[i] == '}')
                                {
                                    sc.AddField("#Count#", ele_idx, sdf);
                                    idx_o = i + 1;
                                    sdf.SerializeChain.Pop();
                                    goto LABEL1;
                                }
                            }
                            sc.AddField("#Count#", ele_idx, sdf);
                            sdf.SerializeChain.Pop();
                            Debug.LogError("");
                            break;
                        }
                    case SerializedObjectType.Reference:
                        sc = new SerializedClass(null, t, sdf);
                        for (int i = idx_save0; i < str.Length; i++)
                        {
                            if (str[i] == '}')
                            {
                                string path = str[idx_save0..i];

                                if (path[0] == '.')
                                    path = sdf.Name + path[1..];

                                //这里不解析，防止循环解析
                                SerializedClass temp0 = sdf.Space.GetSerializedObjectWithPath(path, false);

                                if (temp0 != null)
                                {
                                    sc.RefSource = temp0;
                                    SerializedClass ref_sc = sc.GetRefrenceSource();
                                    if (ref_sc != null)
                                        ref_sc.RefCount++;
                                    else
                                        sdf.FieldsWaitForAdd.Enqueue(sc);
                                }
                                //可能引用对象还未解析
                                //暂存这一次解析
                                else
                                {
                                    sc.Alias = path;
                                    sc.RefSource = null;
                                    sdf.FieldsWaitForAdd.Enqueue(sc);
                                }

                                idx_o = i + 1;
                                goto LABEL1;
                            }
                        }
                        Debug.LogError("");
                        break;
                    case SerializedObjectType.KeyValuePair:
                        sc = new SerializedClass(null, t, sdf);
                        sdf.SerializeChain.Push(sc);

                        SerializedClass key = Parse(str, idx_save0, sdf, out _);
                        if (key == null)
                        {
                            sc.AddField("#Key#", new SerializedClass(null, SerializedObjectType.Error, sdf));
                            sc.AddField("#Value#", new SerializedClass(null, SerializedObjectType.Error, sdf));
                            Debug.LogError("");
                            sdf.SerializeChain.Pop();
                            goto LABEL1;
                        }
                        sc.AddField("#Key#", key);

                        SerializedClass value = Parse(str, idx_save1, sdf, out idx_o);
                        if (value == null)
                        {
                            sc.AddField("#Value#", new SerializedClass(null, SerializedObjectType.Error, sdf));
                            idx_o = idx;
                            Debug.LogError("");
                        }
                        else
                            sc.AddField("#Value#", value);

                        idx_o++;
                        sdf.SerializeChain.Pop();
                        goto LABEL1;
                    case SerializedObjectType.Tuple:
                        sc = new SerializedClass(null, SerializedObjectType.Tuple, sdf);
                        sdf.SerializeChain.Push(sc);

                        int cnt = 0;
                        for (sc.AddField($"#{cnt++}#", Parse(str, idx_save0, sdf, out int i)); i < str.Length;)
                        {
                            if (str[i] == ';' && str[i + 1] != ')')
                                sc.AddField($"#{cnt++}#", Parse(str, i + 1, sdf, out i));
                            else if (str[i] == ')')
                            {
                                sc.AddField($"#Count#", cnt, sdf);
                                idx_o = i + 1;
                                break;
                            }
                            else if (str[i] == ';' && str[i + 1] == ')')
                            {
                                sc.AddField($"#Count#", cnt, sdf);
                                idx_o = i + 2;
                                break;
                            }
                            else
                                i++;
                        }

                        sdf.SerializeChain.Pop();
                        goto LABEL1;
                    case SerializedObjectType.Basic:
                        for (int i = idx_save0; i < str.Length; i++)
                        {
                            //记得排除vector类型(x,y)
                            if ((str[i] == ',' && str[idx_save0] != '(') || str[i] == ':' || str[i] == '}' || str[i] == '[' || str[i] == ')' || str[i] == ';' || str[i] == '<')
                            {
                                if (str[i] == ')')
                                    i++;
                                sc = new SerializedClass(str[idx_save0..i], t, sdf);
                                idx_o = i;
                                goto LABEL1;
                            }
                        }
                        Debug.LogError("");
                        break;
                    case SerializedObjectType.Enum:
                        for (int i = idx_save0; i < str.Length; i++)
                        {
                            if (str[i] == ',' || str[i] == ':' || str[i] == '}' || str[i] == '[' || str[i] == '<' || str[i] == ')')
                            {
                                sc = new SerializedClass(str[idx_save0..i], t, sdf);
                                idx_o = i;
                                goto LABEL1;
                            }
                        }
                        Debug.LogError("");
                        break;
                    case SerializedObjectType.Null:
                        sc = new SerializedClass(null, t, sdf);
                        idx_o = idx_save1 + 2;
                        break;
                    case SerializedObjectType.Error:
                        sc = new SerializedClass(null, t, sdf);
                        break;
                    case SerializedObjectType.Empty:
                        sc = new SerializedClass(null, t, sdf);
                        idx_o = idx_save0 + 1;
                        break;
                    default:
                        Debug.LogError("");
                        break;
                }
            LABEL1:
                sc ??= new SerializedClass(null, SerializedObjectType.Error, sdf);
                //设置特性
                sc.Attributes = atbs;
                //补充子类型
                if (parent != null)
                {
                    if (parent.ObjectType.HasFlag(SerializedObjectType.Enumerator))
                        sc.ObjectType |= SerializedObjectType.Element;
                    else if (parent.ObjectType.HasFlag(SerializedObjectType.KeyValuePair))
                    {
                        if (parent.Fields.ContainsKey("#Key#"))
                            sc.ObjectType |= SerializedObjectType.DictionaryValue;
                        else
                            sc.ObjectType |= SerializedObjectType.DictionaryKey;
                    }
                }

                return sc;
            }
            catch { throw; }
        }


        internal object UnSerialize(Type t, UnSerializeOption opt, params object[] args)
        {
            object inst = null;

            if (opt.HasFlag(UnSerializeOption.Copy))
                CopyCount++;

            if (ObjectType.HasFlag(SerializedObjectType.Error))
                return default;
            else if (ObjectType.HasFlag(SerializedObjectType.Null))
                return null;
            //引用类型不考虑Derive
            if (ObjectType.HasFlag(SerializedObjectType.Reference))
            {
                if (CopyCount == 0)
                {
                    if (SDF.Space.CheckSerializedObject(GetRefrenceSource(), out inst))
                        goto END;
                    else
                    {
                        //LogManager.Warn("Warn0");
                    }
                }
                else
                {
                    if (SDF.Space.CopyIncludeObjects.TryGet((GetRefrenceSource(), CopyCount), out inst))
                        goto END;
                    else
                    {
                        //LogManager.Warn("Warn1");
                    }
                }

                inst = GetRefrenceSource().UnSerialize(t, opt, args);
                goto END;
            }

            //首先检查自身是否已经反序列化过了。而在Copy模式下，即使自身已经反序列化过了，也要重新反序列化
            if (!opt.HasFlag(UnSerializeOption.Copy) && SDF.Space.CheckSerializedObject(this, out object obj))
                inst = obj;
            else if (opt.HasFlag(UnSerializeOption.Copy) && SDF.Space.CopyIncludeObjects.TryGet((this, CopyCount), out obj))
                inst = obj;
            else
            {
                //检查是否为派生
                //这里允许覆盖父字段的派生对象，也就是对派生目标重定向
                if (Attributes != null && Attributes.Exists(x => x is DeriveFrom))
                {
                    DeriveFrom atb = Attributes.First(x => x is DeriveFrom) as DeriveFrom;
                    SerializedClass sc = SDF.Space.GetSerializedObjectWithPath(atb.Path, true);
                    if (sc != null)
                    {
                        DeriveField = sc.GetRefrenceSource();
                        //类型重载
                        Type __t = null;
                        if (Attributes.FirstOrDefault(x => x is TypeDef) is TypeDef _typ_atb)
                            __t = Type.GetType(_typ_atb.TypeName);

                        //获取反序列化方法
                        GeneralUnSerializeMethod _gum = SerializeTool.GetGeneralUnSerializeMethod(__t ?? t);
                        inst = _gum(this, __t ?? t, opt | UnSerializeOption.Derive, args);
                    }
                    else
                    {
                        LogManager.Error("SerializeSystem", $"该字段派生自一个不存在的字段：字段{atb.Path}不存在", args: ("发生错误字段", GetPath()));
                        inst = default;
                    }
                    goto END;
                }

                //类型重载
                Type _t = null;
                if (Attributes?.FirstOrDefault(x => x is TypeDef) is TypeDef typ_atb)
                    _t = typ_atb.GetTypeInst();

                //获取反序列化方法
                GeneralUnSerializeMethod gum = SerializeTool.GetGeneralUnSerializeMethod(_t ?? t);
                inst = gum(this, _t ?? t, opt & ~UnSerializeOption.Derive, args);
            }

        END:
            if (opt.HasFlag(UnSerializeOption.Copy))
                CopyCount--;

            return inst;
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="opt">反序列化选项</param>
        /// <typeparam name="T">反序列化类型，指明要生成的类型</typeparam>
        /// <returns>反序列化结果</returns>
        internal T UnSerialize<T>(UnSerializeOption opt, params object[] args)
        {
            return (T)UnSerialize(typeof(T), opt, args);
        }

        /// <summary>
        /// 对字段反序列化
        /// </summary>
        /// <typeparam name="T">反序列化类型，指明要生成的类型</typeparam>
        /// <param name="field">字段名</param>
        /// <param name="dflt">当字段不存在时，方法应该返回的值</param>
        /// <param name="opt">反序列化选项</param>
        /// <returns>反序列化结果</returns>
        public T UnSerializeField<T>(string field, T dflt, UnSerializeOption opt, params object[] args)
        {
            if (Fields.ContainsKey(field))
                return Fields[field].UnSerialize<T>(opt & ~UnSerializeOption.Derive, args);
            //优先返回派生字段
            else if (opt.HasFlag(UnSerializeOption.Derive))
            {
                T inst = DeriveField.UnSerializeField(field, dflt, UnSerializeOption.Copy, args);

                if (!opt.HasFlag(UnSerializeOption.Copy))
                    SDF.Space.CopyIncludeObjects.Clear();

                return inst;
            }
            //二次派生
            else if (Attributes != null && Attributes.Exists(x => x is DeriveFrom))
            {
                DeriveFrom atb = Attributes.First(x => x is DeriveFrom) as DeriveFrom;
                SerializedClass sc = SDF.Space.GetSerializedObjectWithPath(atb.Path, true);
                if (sc != null)
                {
                    DeriveField = sc.GetRefrenceSource();
                    return sc.UnSerializeField(field, dflt, opt, args);
                }
                else
                {
                    LogManager.Error("SerializeSystem", $"该字段派生自一个不存在的字段：字段{atb.Path}不存在", args: ("发生错误的字段", GetPath()));
                }
            }

            return dflt;
        }

        /// <summary>
        /// 对字段反序列化
        /// </summary>
        /// <typeparam name="T">反序列化类型，指明要生成的类型</typeparam>
        /// <param name="field">字段名</param>
        /// <param name="opt">反序列化选项</param>
        /// <returns>反序列化结果</returns>
        public T UnSerializeField<T>(string field, UnSerializeOption opt, params object[] args)
        {
            return UnSerializeField<T>(field, default, opt, args);
        }

        /// <summary>
        /// 从一个现存字段中派生
        /// </summary>
        /// <typeparam name="T">反序列化类型，指明要生成的类型</typeparam>
        /// <param name="sc">要派生的字段</param>
        /// <returns>反序列化结果</returns>
        /// <remarks>如果一个字段在this中存在，那么就根据this中的内容反序列化；如果不存在，那么就从<paramref name="sc"/>中反序列化同名字段</remarks>
        public T DeriveFrom<T>(SerializedClass sc, params object[] args)
        {
            DeriveField = sc;
            //获取反序列化方法
            GeneralUnSerializeMethod _gum = SerializeTool.GetGeneralUnSerializeMethod(typeof(T));
            return (T)_gum(this, typeof(T), UnSerializeOption.Derive, args);
        }

        /// <summary>
        /// 通过反序列化的方式，复制一个实例
        /// </summary>
        /// <typeparam name="T">反序列化类型</typeparam>
        /// <returns>反序列化结果</returns>
        public T Copy<T>(params object[] args)
        {
            return UnSerialize<T>(UnSerializeOption.Copy, args);
        }


        /// <summary>
        /// 生成字符串（格式化）
        /// </summary>
        /// <param name="str">格式化字符串</param>
        /// <param name="fp">格式化器</param>
        /// <returns>格式化结果</returns>
        /// <remarks>
        /// str参数应符合格式：$"{ahead},{inline}"，ahead是int类型，它指明格式化时的缩进长度，
        /// inline时bool类型，它指明是否为内联模式，在内联模式下不会生成字段名
        /// </remarks>
        public string ToString(string str, IFormatProvider fp)
        {
            return (fp as ICustomFormatter).Format(str, this, fp);
        }

        /// <summary>
        /// 生成字符串（格式化）
        /// </summary>
        /// <param name="str">格式化字符串</param>
        /// <returns>格式化结果</returns>
        public string ToString(string str)
        {
            return ToString(str, new SerializeFormatter());
        }

        /// <summary>
        /// 生成字符串（格式化）
        /// </summary>
        /// <returns>格式化结果</returns>
        public override string ToString()
        {
            return ToString("0,false", new SerializeFormatter());
        }
    }
}
