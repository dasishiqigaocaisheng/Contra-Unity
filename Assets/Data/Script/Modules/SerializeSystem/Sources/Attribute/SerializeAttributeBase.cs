using System;
using Modules.LogSystem;

namespace Modules.SerializeSystem
{
    internal abstract class SerializeAttributeBase
    {
        public override abstract string ToString();

        public static SerializeAttributeBase Parse(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                LogManager.Error("SerializeSystem.SerializeAttributeBase", "序列化特性解析时出错：参数（str）是空或null");
                return null;
            }

            if (str[0..6] == "Matrix")
            {
                string[] dims = str[7..^1].Split(',', StringSplitOptions.RemoveEmptyEntries);

                Matrix matb = new Matrix();
                foreach (string dim in dims)
                    matb.DimensionSize.Add(int.Parse(dim));
                matb.Rank = dims.Length;

                return matb;
            }
            else if (str[0..6] == "Derive")
            {
                DeriveFrom datb = new DeriveFrom();
                datb.Path = str[8..^2];

                return datb;
            }
            else if (str[0..7] == "TypeDef")
            {
                string[] substr = str[8..^1].Split(',');
                if (substr.Length != 2)
                {
                    LogManager.Error("SerializeSystem.SerializeAttributeBase", $"解析特性时出错，子字符串的数量（{substr.Length}）不为2");
                    return null;
                }

                TypeDef typatb = new TypeDef();
                typatb.AssemblyName = substr[0][1..^1];
                typatb.TypeName = substr[1][1..^1];

                return typatb;
            }
            else if (str[0..16] == "ElementTypeInfer")
                return new ElementTypeInfer();
            else
            {
                LogManager.Error("SerializeSystem.SerializeAttributeBase", $"序列化特性解析时出错：未知的特性类型（{str}）");
                return null;
            }
        }
    }
}
