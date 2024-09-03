using System;
using System.Reflection;
using Modules.LogSystem;

namespace Modules.SerializeSystem
{
    internal class TypeDef : SerializeAttributeBase
    {
        //程序集名
        public string AssemblyName { get; set; }

        //类型名
        public string TypeName { get; set; }

        //获取类型实例
        public Type GetTypeInst()
        {
            try
            {
                Assembly asm = Assembly.Load(AssemblyName);
                return asm.GetType(TypeName);
            }
            catch (Exception ex)
            {
                LogManager.Error("SerializeSystem.TypeDef", "获取类型实例时出现异常", args: ("异常信息", ex));
            }
            return null;
        }


        public override string ToString()
        {
            return $"TypeDef(\"{TypeName}\",\"{TypeName}\")";
        }
    }
}
