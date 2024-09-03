namespace Modules.SerializeSystem
{
    /// <summary>
    /// 用户自定义的可序列化类都要实现该接口
    /// </summary>
    public interface IAllowSerialize
    {
        /// <summary>
        /// 生成序列化字段
        /// </summary>
        /// <param name="alias">别名 对于要“延迟定义”的字段，在“延迟定义”时会使用该参数作为字段名</param>
        /// <param name="cfg">序列化配置</param>
        /// <param name="sdf">要写入的序列化文件</param>
        /// <returns>序列化字段</returns>
        /// <remarks>
        /// <para>
        /// 该方法的编写必须遵循一定的模板，即：以<c>SerializeBegin</c>方法开头并以<c>SerializeEnd</c>方法结尾，<c>SerializeBegin</c>方法会返回一个序列化字段，
        /// 而这个由<c>SerializeBegin</c>方法生成的序列化字段必须作为<c>SerializeEnd</c>的参数，且最终作为返回值返回。
        /// </para>
        /// <para>下面是一个编写模板：</para>
        /// <code>
        /// SerializedClass Serialize(string alias, SerializeConfig cfg, SerializedDataFile sdf)
        /// {
        ///     if (sdf.SerializeBegin(alias, this, cfg, out SerializedClass sc))
        ///     {
        ///         ... //你的序列化代码
        ///     }
        ///     return sdf.SerializeEnd(sc);
        /// }
        /// </code>
        /// </remarks>
        public SerializedClass Serialize(string alias, SerializeConfig cfg, SerializedDataFile sdf)
        {
            return new SerializedClass("", SerializedObjectType.Error, sdf);
        }

        /// <summary>
        /// 反序列化覆写
        /// </summary>
        /// <param name="sc">序列化字段</param>
        /// <param name="sdf">关联的</param>
        /// <returns>必须是覆写过后的对象自身（this）</returns>
        /// <remarks>
        /// 该方法中，应根据序列化字段<paramref name="sc"/>中的子字段信息来覆写对象的相关数据，来生成反序列化的类
        /// </remarks>
        public object UnSerializeOverWrite(SerializedClass sc, UnSerializeOption opt)
        {
            return this;
        }
    }
}
