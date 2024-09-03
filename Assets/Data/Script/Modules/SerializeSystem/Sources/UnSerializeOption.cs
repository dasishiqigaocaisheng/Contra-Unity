using System;

namespace Modules.SerializeSystem
{
    [Flags]
    public enum UnSerializeOption
    {
        None,
        //这个字段是从某个字段派生的
        Derive = 1,
        //以复制的方式反序列化（将不会在数据空间中留下记录）
        Copy = 1 << 1,
        //当反序列化枚举类型时，根据TypeAttribute来推断每个元素的类型
        InferElementType = 1 << 2
    }
}
