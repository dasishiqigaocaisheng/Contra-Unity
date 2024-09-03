namespace Modules.SerializeSystem
{
    public delegate string ElementAliasGetter(object obj, int idx, SerializedDataFile sdf);
    public delegate string KeyAliasGetter(object obj, SerializedDataFile sdf);
    public delegate string ValueAliasGetter(object obj, SerializedDataFile sdf);

    /// <summary>
    /// 序列化配置
    /// 该结构用于配置序列化过程
    /// </summary>
    public struct SerializeConfig
    {
        //序列化选项
        public SerializeConfigOption ConfigOption;

        //获取元素别名
        public ElementAliasGetter CustomGetElementAlias;

        //获取Key别名
        public KeyAliasGetter CustomGetKeyAlias;

        //获取Value别名
        public ValueAliasGetter CustomGetValueAlias;

        //格式化选项
        public SerializeFormatOption FormatOption;

        //偏好列数（格式化选项）
        //指明格式化时，其中元素应该多少列为一行
        public int PreferColumn;

        public bool IsDelayDefine { get => ConfigOption.HasFlag(SerializeConfigOption.DelayDefine); }
        public bool IsElementDelayDefine { get => ConfigOption.HasFlag(SerializeConfigOption.ElementsDelayDefine); }
        public bool IsKeyDelayDefine { get => ConfigOption.HasFlag(SerializeConfigOption.KeyDelayDefine); }
        public bool IsValueDelayDefine { get => ConfigOption.HasFlag(SerializeConfigOption.ValueDelayDefine); }
        public bool IsExtern { get => ConfigOption.HasFlag(SerializeConfigOption.Extern); }

        public static SerializeConfig Default { get => new SerializeConfig(SerializeConfigOption.Default); }

        /*
         *构造函数
         */
        public SerializeConfig(SerializeConfigOption cfg_opt = SerializeConfigOption.Default,
                                ElementAliasGetter elementalias_getter = null,
                                KeyAliasGetter keyalias_getter = null,
                                ValueAliasGetter valuealias_getter = null,
                                SerializeFormatOption format_option = SerializeFormatOption.Auto,
                                int prefer_column = 1)
        {
            ConfigOption = cfg_opt;
            FormatOption = format_option;
            PreferColumn = prefer_column;

            if (cfg_opt.HasFlag(SerializeConfigOption.ElementsDelayDefine))
                CustomGetElementAlias = elementalias_getter;
            else
                CustomGetElementAlias = null;

            if (cfg_opt.HasFlag(SerializeConfigOption.KeyDelayDefine))
                CustomGetKeyAlias = keyalias_getter;
            else
                CustomGetKeyAlias = null;

            if (cfg_opt.HasFlag(SerializeConfigOption.ValueDelayDefine))
                CustomGetValueAlias = valuealias_getter;
            else
                CustomGetValueAlias = null;
        }

        public string GetElementAlias(object obj, int idx, SerializedDataFile sdf)
        {
            return CustomGetElementAlias != null ? CustomGetElementAlias(obj, idx, sdf) : sdf.Space.NextAlias;
        }

        public string GetKeyAlias(object obj, SerializedDataFile sdf)
        {
            return CustomGetKeyAlias != null ? CustomGetKeyAlias(obj, sdf) : sdf.Space.NextAlias;
        }

        public string GetValueAlias(object obj, SerializedDataFile sdf)
        {
            return CustomGetValueAlias != null ? CustomGetValueAlias(obj, sdf) : sdf.Space.NextAlias;
        }
    }
}
