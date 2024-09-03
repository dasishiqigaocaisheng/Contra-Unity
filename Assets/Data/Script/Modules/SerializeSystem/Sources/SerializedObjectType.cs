using System;

namespace Modules.SerializeSystem
{
    /*
     *定义序列化类的类型
     */
    [Flags]
    public enum SerializedObjectType
    {
        None = 0,
        //一般的引用类型
        Class = 1,
        //C#基础类型，Vector类型，string，Enum
        Basic = 1 << 1,
        //字符串
        String = 1 << 2 | Basic,
        //枚举
        Enum = 1 << 3 | Basic,
        //结构体
        Struct = 1 << 4,
        //枚举器
        Enumerator = 1 << 5,
        //元素类型，该类是某个类的元素（如数组的元素）
        Element = 1 << 6,
        //根类型，SDF的根节点
        Root = 1 << 7,
        //该类是KeyValuePair
        KeyValuePair = 1 << 8 | Struct,
        //该类是字典的Key
        DictionaryKey = 1 << 9,
        //该类是字典的Value
        DictionaryValue = 1 << 10,
        //ValueTuple类型
        Tuple = 1 << 11,
        //该类是对其它某个序列化类的引用
        Reference = 1 << 12,
        //该类的值是null
        Null = 1 << 13,
        //错误类型，在序列化或反序列化时出错
        Error = 1 << 14,
        //空置类型，没有任何子字段的序列化类的或者只有0个元素的Enumerator在反序列化时具有这种类型
        Empty = 1 << 16
    }
}
