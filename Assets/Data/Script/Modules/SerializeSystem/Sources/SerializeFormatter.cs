using System;
using System.Text;

namespace Modules.SerializeSystem
{
    internal class SerializeFormatter : ICustomFormatter, IFormatProvider
    {
        public object GetFormat(Type formatType)
        {
            if (formatType is ICustomFormatter)
                return this;
            else
                return null;
        }

        //格式化字符串
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg is not SerializedClass sc) { /*Error*/ return null; }

            string[] param = format.Split(',');

            //获取缩进长度
            int ahead_len = int.Parse(param[0]);
            string ahead = new string('\t', ahead_len);
            //获取是否是内联模式
            bool inline_mode = bool.Parse(param[1]);

            if (sc.ObjectType.HasFlag(SerializedObjectType.Null))
                return ahead + $"[{sc.Name}]=#Null#";
            else if (sc.ObjectType.HasFlag(SerializedObjectType.Basic))
            {
                if (!inline_mode)
                    return ahead + $"[{sc.Name}]={sc.Alias}";
                else
                    return ahead + $"{sc.Alias}";
            }
            //KeyValuePair类型应该永远保持inline模式
            else if (sc.ObjectType.HasFlag(SerializedObjectType.KeyValuePair))
                return ahead + $"{{{sc["#Key#"].ToString($"{0},true", formatProvider)} : {sc["#Value#"].ToString($"{0},true", formatProvider)}}}";
            else if (sc.ObjectType.HasFlag(SerializedObjectType.Enumerator))
            {
                int len = int.Parse(sc["#Count#"].Alias);
                int column = len;
                bool is_compact = sc.FormatOption.HasFlag(SerializeFormatOption.Compact);

                //自动格式化
                if (sc.FormatOption.HasFlag(SerializeFormatOption.Auto))
                {
                    /*if (sc.ObjectType.HasFlag(SerializedObjectType.Element))
                        is_compact = true;
                    else if (len <= 16 && (sc.ElementType.HasFlag(SerializedObjectType.Basic) || sc.ElementType.HasFlag(SerializedObjectType.None)))
                        is_compact = true;*/
                    is_compact = true;
                }

                if (!is_compact)
                    column = int.Parse(sc["#PreferColumn#"].Alias);

                StringBuilder sb = new StringBuilder(1024);

                sb.Append(ahead);
                if (!inline_mode)
                {
                    if (sc.Attributes != null)
                    {
                        foreach (SerializeAttributeBase atb in sc.Attributes)
                            sb.AppendLine(atb.ToString()).Append(ahead);
                    }

                    if (is_compact)
                        sb.Append($"[{sc.Name}]={{");
                    else
                        sb.AppendLine($"[{sc.Name}]=\r\n{ahead}{{").Append(ahead);
                }
                else
                {
                    if (sc.Attributes != null)
                    {
                        if (is_compact)
                        {
                            foreach (SerializeAttributeBase atb in sc.Attributes)
                                sb.Append($"{atb}");
                        }
                        else
                        {
                            foreach (SerializeAttributeBase atb in sc.Attributes)
                                sb.AppendLine($"{atb}").Append(ahead);
                        }
                    }
                    sb.Append("{");
                }


                for (int i = 0; i < len; i += column)
                {
                    if (!is_compact)
                        sb.Append("\t");

                    for (int j = 0; j < column; j++)
                        sb.Append(sc[(i + j)].ToString($"0,true", formatProvider)).Append(",");

                    if (!is_compact)
                        sb.Append("\r\n").Append(ahead);
                }

                sb.Remove(sb.Length - 1, 1);
                sb.Append("}");

                return sb.ToString();
            }
            else if (sc.ObjectType.HasFlag(SerializedObjectType.Reference))
                return ahead + $"[{sc.Name}]={{{sc.RefSource.GetPath(sc.SDF)}}}";
            else if (sc.ObjectType.HasFlag(SerializedObjectType.Class) || sc.ObjectType.HasFlag(SerializedObjectType.Struct))
            {
                StringBuilder sb = new StringBuilder(1024);

                sb.Append(ahead);
                if (!inline_mode)
                {
                    if (sc.Attributes != null)
                    {
                        foreach (SerializeAttributeBase atb in sc.Attributes)
                            sb.AppendLine(atb.ToString()).Append(ahead);
                    }
                    sb.AppendLine($"[{sc.Name}]=\r\n{ahead}{{");
                }
                else
                {
                    if (sc.Attributes != null)
                    {
                        foreach (SerializeAttributeBase atb in sc.Attributes)
                            sb.Append(ahead).Append(atb).AppendLine("{");
                    }
                }

                foreach (SerializedClass sc_temp in sc.Fields.Values)
                    sb.AppendLine(sc_temp.ToString($"{int.Parse(param[0]) + 1},false", formatProvider));

                sb.Append(ahead).Append("}");

                return sb.ToString();
            }
            else if (sc.ObjectType.HasFlag(SerializedObjectType.Tuple))
            {
                StringBuilder sb = new StringBuilder(1024);

                sb.Append(ahead);
                if (!inline_mode)
                {
                    if (sc.Attributes != null)
                    {
                        foreach (SerializeAttributeBase atb in sc.Attributes)
                            sb.AppendLine(atb.ToString()).Append(ahead);
                    }
                    sb.AppendLine($"[{sc.Name}]=\r\n{ahead}(");
                }
                else
                {
                    if (sc.Attributes != null)
                    {
                        foreach (SerializeAttributeBase atb in sc.Attributes)
                            sb.Append(ahead).Append(atb).AppendLine("(");
                    }
                }

                foreach (SerializedClass sc_temp in sc.Fields.Values)
                    sb.AppendLine(sc_temp.ToString($"{int.Parse(param[0]) + 1},true", formatProvider) + ";");

                sb.Append(ahead).Append(")");

                return sb.ToString();
            }
            else if (sc.ObjectType.HasFlag(SerializedObjectType.Error))
                return ahead + $"[{sc.Name}]=#Error#";
            else
            {
                //Error
                return null;
            }
        }
    }
}
