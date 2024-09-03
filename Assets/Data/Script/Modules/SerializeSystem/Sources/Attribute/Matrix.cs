using System.Collections.Generic;

namespace Modules.SerializeSystem
{
    /// <summary>
    /// 矩阵特性
    /// 该特性表明该字段是一个矩阵（多维数组），并指明每一个维度的长度
    /// </summary>
    /// <remarks>基本形式：<![CDATA[<Matrix(x,y,z)>]]>【x，y，z是第1/2/3个维度的长度】</remarks>
    internal class Matrix : SerializeAttributeBase
    {
        //维度
        public int Rank { get; set; }

        //各个维度的长度
        public List<int> DimensionSize { get; set; } = new List<int>();

        public override string ToString()
        {
            string str = "";
            foreach (int dim in DimensionSize)
                str += $"{dim},";
            return $"<Matrix({str[..^1]})>";
        }
    }
}
