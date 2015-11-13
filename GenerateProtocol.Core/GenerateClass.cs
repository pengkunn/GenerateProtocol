using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace GenerateProtocol.Core
{
    public class GenerateClass
    {
        static IList<string> _typeNames = new List<string>();
        static IDictionary<string, string> _exportFiles = new Dictionary<string, string>();
        static string _enumHeadText;
        static string _msgHeadText;

        static public IDictionary<string, string> GenerateAllMessage()
        {
            StringBuilder sb = new StringBuilder();
            Assembly assembly = Assembly.Load("GenerateProtocol.Template");
            Stream enumHeadStream = assembly.GetManifestResourceStream("GenerateProtocol.Template.EnumHead.txt");
            Stream msgHeadStream = assembly.GetManifestResourceStream("GenerateProtocol.Template.MessageHead.txt");
            _enumHeadText = (new StreamReader(enumHeadStream)).ReadToEnd();
            _msgHeadText = (new StreamReader(msgHeadStream)).ReadToEnd();
            foreach (Type type in assembly.ExportedTypes)
            {
                if (type.IsSubclassOf(typeof(BaseType)))
                {
                    GenerateCPlusPlusCodeFromBaseType(type);
                }

                if (type.IsEnum)
                {
                    GenerateEnumCode(type);
                }
            }

            Console.WriteLine("Generate End");

            return _exportFiles;
        }

        static public void GenerateEnumCode(Type enumType)
        {
            if (!enumType.IsEnum)
                return;

            if (_typeNames.Contains(enumType.Name))
                return;

            _typeNames.Add(enumType.Name);

            Console.WriteLine(enumType.Name + " Generate Start");

            StringBuilder sb = new StringBuilder();
            sb.Append(_enumHeadText);
            sb.Append("\n");

            sb.Append("enum " + enumType.Name + "\n");
            sb.Append("{\n");
            foreach (object value in enumType.GetEnumValues())
            {
                sb.AppendFormat("{0} = {1},\n", enumType.GetEnumName(value), (int)value);
            }

            sb.Append("}\n");

            _exportFiles.Add(enumType.Name, sb.ToString());

            Console.WriteLine(enumType.Name + " Generate Success");
        }

        static public string GenerateCPlusPlusCodeFromBaseType(Type baseType)
        {
            if (!baseType.IsSubclassOf(typeof(BaseType)))
                return string.Empty;

            if (_typeNames.Contains(baseType.Name))
                return string.Empty;

            _typeNames.Add(baseType.Name);

            Console.WriteLine(baseType.Name + " Generate Start");

            StringBuilder sb = new StringBuilder();
            sb.Append(_msgHeadText);
            sb.Append("\n");

            sb.Append("struct " + baseType.Name + "_MSG\n");
            sb.Append("{\n");

            //检查是否含有其他子类型，如果有子类型，则先生成子类型的c++代码，放在最代码文本最开始的位置
            //foreach (FieldInfo fieldInfo in baseType.GetFields())
            //{
            //    if (fieldInfo.FieldType.IsSubclassOf(typeof(BaseType)))
            //    {
            //        string subType = GenerateCPlusPlusCodeFromBaseType(fieldInfo.FieldType);
            //        sb.Insert(0, subType);
            //    }

            //    if (fieldInfo.FieldType.IsGenericType)
            //    {
            //        foreach (Type GenericTypeArgument in fieldInfo.FieldType.GenericTypeArguments)
            //        {
            //            string subType = GenerateCPlusPlusCodeFromBaseType(GenericTypeArgument);
            //            sb.Insert(0, subType);
            //        }
            //    }
            //}


            StringBuilder sbFields = new StringBuilder();
            StringBuilder sbToStream = new StringBuilder();
            StringBuilder sbFromStream = new StringBuilder();

            if (baseType.IsSubclassOf(typeof(BaseMessage)))
            {
                sbFields.Append("static const u_int msg_id=" + baseType.Name + ";\n");

                sbFields.Append("static const char* get_name()\n");
                sbFields.Append("{\n");
                sbFields.AppendFormat("return \"{0}\";\n", baseType.Name);
                sbFields.Append("}\n");

                sbToStream.Append("void to_stream(XMsgPacker& packer){\n");
                //sbToStream.Append("packer.write_string(\"msg_id\");\n");
                //sbToStream.Append("packer.write_u_int32(" + baseType.Name + ");\n");


                sbFromStream.Append("void from_stream(XMessageBlock * stream, XMsgParse & parse) {\n");
                sbFromStream.Append("for (i = 0; i < " + (baseType.GetFields().Count() + 1).ToString() + "; i++)\n");
                sbFromStream.Append("{\n");
                sbFromStream.Append("string strFiled = parse.read_string();\n");
                sbFromStream.Append("readValue(strFiled,stream,parse);\n");
                sbFromStream.Append("}\n");
                sbFromStream.Append("}\n");
                sbFromStream.Append("void readValue(string strFiled ,XMessageBlock * stream, XMsgParse & parse)\n");
                sbFromStream.Append("{\n");


            }

            foreach (FieldInfo fieldInfo in baseType.GetFields())
            {
                sbFields.Append(GenerateField(fieldInfo));
                if (baseType.IsSubclassOf(typeof(BaseMessage)))
                {
                    sbToStream.Append(GenerateToStreamMethod(fieldInfo));
                    sbFromStream.Append(GenerateFromStreamMethod(fieldInfo));
                }

            }

            if (baseType.IsSubclassOf(typeof(BaseMessage)))
            {
                sbToStream.Append("}\n");
                sbFromStream.Append("}\n");
            }

            sb.Append(sbFields).Append(sbToStream).Append(sbFromStream);

            sb.Append("}\n");

            _exportFiles.Add(baseType.Name, sb.ToString());

            Console.WriteLine(baseType.Name + " Generate Success");

            return sb.ToString();
        }

        static string GenerateField(FieldInfo fieldInfo)
        {
            StringBuilder sb = new StringBuilder();
            AnnotateAttribute annotateAttribute = fieldInfo.GetCustomAttribute<AnnotateAttribute>(false);

            if (annotateAttribute != null)
                sb.AppendFormat("//{0} \n", annotateAttribute.Text);

            if (fieldInfo.FieldType.IsValueType)
            {
                if (fieldInfo.FieldType == typeof(Int64))
                {
                    sb.Append("int64 " + fieldInfo.Name + ";\n");
                }
                else if (fieldInfo.FieldType == typeof(Int32))
                {
                    sb.Append("int32 " + fieldInfo.Name + ";\n");
                }
                else if (fieldInfo.FieldType == typeof(Int16))
                {
                    sb.Append("int16 " + fieldInfo.Name + ";\n");
                }
                else if (fieldInfo.FieldType == typeof(Byte))
                {
                    sb.Append("int8 " + fieldInfo.Name + ";\n");
                }
                else if (fieldInfo.FieldType == typeof(UInt64))
                {
                    sb.Append("uint64 " + fieldInfo.Name + ";\n");
                }
                else if (fieldInfo.FieldType == typeof(UInt32))
                {
                    sb.Append("uint32 " + fieldInfo.Name + ";\n");
                }
                else if (fieldInfo.FieldType == typeof(UInt16))
                {
                    sb.Append("uint16 " + fieldInfo.Name + ";\n");
                }
                else if (fieldInfo.FieldType == typeof(SByte))
                {
                    sb.Append("int8 " + fieldInfo.Name + ";\n");
                }
            }
            else if (fieldInfo.FieldType.IsClass)
            {
                if (fieldInfo.FieldType == typeof(string))
                {
                    sb.Append("string " + fieldInfo.Name + ";\n");
                }
                else if (fieldInfo.FieldType.IsGenericType)
                {
                    sb.Append("list<" + fieldInfo.FieldType.GenericTypeArguments.ElementAt(0).Name + "> " + fieldInfo.Name + ";\n");
                }
                else if (fieldInfo.FieldType.IsSubclassOf(typeof(BaseType)))
                {
                    sb.Append(fieldInfo.FieldType.Name + " " + fieldInfo.Name + ";\n");
                }

            }




            return sb.ToString();

        }

        static string GenerateToStreamMethod(FieldInfo fieldInfo)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("packer.write_string(\"" + fieldInfo.Name + "\");\n");

            if (fieldInfo.FieldType.IsValueType)
            {
                if (fieldInfo.FieldType == typeof(Int64))
                {
                    sb.Append("packer.write_int64(" + fieldInfo.Name + ");\n");
                }
                else if (fieldInfo.FieldType == typeof(Int32))
                {
                    sb.Append("packer.write_int32(" + fieldInfo.Name + ");\n");
                }
                else if (fieldInfo.FieldType == typeof(Int16))
                {
                    sb.Append("packer.write_int16(" + fieldInfo.Name + ");\n");
                }
                else if (fieldInfo.FieldType == typeof(Byte))
                {
                    sb.Append("packer.write_int8(" + fieldInfo.Name + ");\n");
                }
                else if (fieldInfo.FieldType == typeof(UInt64))
                {
                    sb.Append("packer.write_uint64(" + fieldInfo.Name + ");\n");
                }
                else if (fieldInfo.FieldType == typeof(UInt32))
                {
                    sb.Append("packer.write_uint32(" + fieldInfo.Name + ");\n");
                }
                else if (fieldInfo.FieldType == typeof(UInt16))
                {
                    sb.Append("packer.write_uint16(" + fieldInfo.Name + ");\n");
                }
                else if (fieldInfo.FieldType == typeof(SByte))
                {
                    sb.Append("packer.write_int8(" + fieldInfo.Name + ");\n");
                }
            }
            else if (fieldInfo.FieldType.IsClass)
            {
                if (fieldInfo.FieldType == typeof(string))
                {
                    sb.Append("packer.write_string(" + fieldInfo.Name + ");\n");
                }
                else if (fieldInfo.FieldType.IsGenericType)
                {
                    sb.Append("packer.write_string(\"" + fieldInfo.FieldType.GenericTypeArguments.ElementAt(0).Name + "\");\n");
                    sb.Append("packer.write_u_short(" + fieldInfo.Name + ".size());\n");
                    sb.Append("for(auto e : " + fieldInfo.Name + ")\n");
                    sb.Append("{\n");
                    sb.Append("e.to_stream(packer);\n");
                    sb.Append("}\n");
                }
                else if (fieldInfo.FieldType.IsSubclassOf(typeof(BaseType)))
                {
                    sb.Append("packer.write_string(\"" + fieldInfo.FieldType.Name + "\");\n");
                    sb.Append(fieldInfo.Name + ".to_stream(packer);\n");
                }

            }
            return sb.ToString();
        }

        static string GenerateFromStreamMethod(FieldInfo fieldInfo)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("if (strFiled == \"msg_id\")\n");
            sb.Append("{\n");
            sb.Append("msg_id = parse.read_u_int32();\n");
            sb.Append("}\n");
            //sb.Append("string str" + fieldInfo.Name + " = parse.read_string();\n");
            sb.Append("else if (strFiled == \"" + fieldInfo.Name + "\")\n");
            sb.Append("{\n");
            if (fieldInfo.FieldType.IsValueType)
            {
                if (fieldInfo.FieldType == typeof(Int64))
                {
                    sb.Append(fieldInfo.Name + " = parse.read_int64();\n");
                }
                else if (fieldInfo.FieldType == typeof(Int32))
                {
                    sb.Append(fieldInfo.Name + " = parse.read_int32();\n");
                }
                else if (fieldInfo.FieldType == typeof(Int16))
                {
                    sb.Append(fieldInfo.Name + " = parse.read_int16();\n");
                }
                else if (fieldInfo.FieldType == typeof(Byte))
                {
                    sb.Append(fieldInfo.Name + " = parse.read_int8();\n");
                }
                else if (fieldInfo.FieldType == typeof(UInt64))
                {
                    sb.Append(fieldInfo.Name + " = parse.read_u_int64();\n");
                }
                else if (fieldInfo.FieldType == typeof(UInt32))
                {
                    sb.Append(fieldInfo.Name + " = parse.read_u_int32();\n");
                }
                else if (fieldInfo.FieldType == typeof(UInt16))
                {
                    sb.Append(fieldInfo.Name + " = parse.read_u_int16();\n");
                }
                else if (fieldInfo.FieldType == typeof(SByte))
                {
                    sb.Append(fieldInfo.Name + " = parse.read_u_int8();\n");
                }
            }
            else if (fieldInfo.FieldType.IsClass)
            {
                if (fieldInfo.FieldType == typeof(string))
                {
                    sb.Append(fieldInfo.Name + " = parse.read_string();\n");
                }
                else if (fieldInfo.FieldType.IsGenericType)
                {
                    sb.Append("u_short len = parse.read_u_short();\n");
                    sb.Append("string type = parse.read_string();\n");
                    sb.Append("if (type == \"" + fieldInfo.FieldType.GenericTypeArguments.ElementAt(0).Name + "\")\n");
                    sb.Append("{\n");
                    sb.Append("for (u_short i = 0; i < len; ++i)\n");
                    sb.Append("{\n");
                    sb.Append(fieldInfo.FieldType.GenericTypeArguments.ElementAt(0).Name + " newType;\n");
                    sb.Append("newType.form_stream(stream, parse);\n");
                    sb.Append("}\n");
                    sb.Append(fieldInfo.Name + ".push_back(newType);\n");
                    sb.Append("}\n");
                }
                else if (fieldInfo.FieldType.IsSubclassOf(typeof(BaseType)))
                {
                    sb.Append("string type = parse.read_string();\n");
                    sb.Append("if (type == \"" + fieldInfo.FieldType.Name + "\")\n");
                    sb.Append("{\n");
                    sb.Append(fieldInfo.Name + ".form_stream(stream, parse);\n");
                    sb.Append("}\n");
                }

            }
            sb.Append("}\n");

            return sb.ToString();
        }
    }
}
