namespace Modules.SerializeSystem
{
    /// <summary>
    /// 派生特性
    /// 该特性表明该字段派生于某个现存字段，并指明这个现存字段的路径
    /// </summary>
    /// <remarks>基本形式：<![CDATA[<Derive("xxx/yyy/zzz")>]]></remarks>
    internal class DeriveFrom : SerializeAttributeBase
    {
        //派生字段的路径
        public string Path { get; set; }

        public override string ToString()
        {
            return $"<Derive(\"{Path}\")>";
        }
    }
}
