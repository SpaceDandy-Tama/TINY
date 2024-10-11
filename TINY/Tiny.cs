//This is not YAML
//As of v0.8:
//MultiDimensional Array's not supported. Dictionary supports built-in value types and string.
//
//MIT License
//
//Copyright (c) 2023 Oğuz Can Soyselçuk
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tiny
{
    internal static class Common
    {
        static bool IsString(ref object o) => IsString(o.GetType());
        static bool IsString(Type type) => type == typeof(string);
        static bool IsArray(ref object o) => o != null && IsArray(o.GetType());
        static bool IsArray(Type type) => type.IsArray;
        internal static bool Is1DArray(ref object o) => IsArray(ref o) && (o as Array).Rank == 1;
        internal static bool Is1DArray(Type type) => IsArray(type) && type.GetArrayRank() == 1;
        internal static bool IsMDArray(ref object o) => IsArray(ref o) && (o as Array).Rank > 1;
        internal static bool IsMDArray(Type type) => IsArray(type) && type.GetArrayRank() > 1;
        internal static bool IsList(ref object o) => o != null && o is IList && IsGenericTypeAssignableFrom(ref o, typeof(List<>));
        internal static bool IsList(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        internal static bool IsDictionary(ref object o) => o != null && o is IDictionary && IsGenericTypeAssignableFrom(ref o, typeof(Dictionary<,>));
        internal static bool IsDictionary(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
        internal static bool IsStruct(ref object o) => o != null && o.GetType().IsValueType && !o.GetType().IsEnum && !o.GetType().IsPrimitive && !IsString(ref o);
        internal static bool IsStruct(Type type) => type.IsValueType && !type.IsEnum && !type.IsPrimitive && !IsString(type);
        internal static bool IsClass(ref object o) => o != null && o.GetType().IsClass && !IsString(ref o) && !IsAnyTypeOfArray(ref o);
        internal static bool IsClass(Type type) => type.IsClass && !IsString(type) && !IsAnyTypeOfArray(type);
        internal static bool HasFields(ref object o) => IsStruct(ref o) || IsClass(ref o);
        internal static bool HasFields(Type type) => IsStruct(type) || IsClass(type);
        static bool IsAnyTypeOfArray(ref object o) => IsArray(ref o) || IsList(ref o) || IsDictionary(ref o);
        static bool IsAnyTypeOfArray(Type type) => IsArray(type) || IsList(type) || IsDictionary(type);
        static bool IsGenericTypeAssignableFrom(ref object o, Type type) => o.GetType().IsGenericType && o.GetType().GetGenericTypeDefinition().IsAssignableFrom(type);
        
    }

    public static class Serializer
    {
        public static string Serialize(object value) => Serialize(ref value, 0);
        public static string Serialize(ref object value, int indent)
        {
            string result = "";

            if (Common.Is1DArray(ref value) || Common.IsList(ref value))
                result += SerializeArray(ref value, indent);
            else if (Common.IsMDArray(ref value))
                result += SerializeMultidimensionalArray(ref value, indent);
            else if (Common.IsDictionary(ref value))
                result += SerializeDictionary(ref value, indent);
            else if (Common.HasFields(ref value))
                result += SerializeClassOrStruct(ref value, indent);
            else if (value == null)
                result += "~\n";
            else
                result += $"{value}\n";

            return result;
        }

        private static string SerializeArray(ref object value, int indent)
        {
            string result = "";
            int length = Common.Is1DArray(ref value) ? (value as Array).Length : (value as IList).Count;
            result += "\n";
            for (int j = 0; j < length; j++)
            {
                object currentValue = (value as IList)[j];
                if (Common.HasFields(ref currentValue))
                    result += $"{Indent(indent + 1)}- \n{Serialize(ref currentValue, indent + 2)}";
                else
                    result += $"{Indent(indent + 1)}- {Serialize(ref currentValue, indent + 1)}";
            }
            return result;
        }

        private static string SerializeMultidimensionalArray(ref object value, int indent)
        {
            throw new Exception("Multidimensional Array's are not supported.");
            return "null\n";
        }

        private static string SerializeDictionary(ref object value, int indent)
        {
            string result = "";
            IDictionary dictionary = value as IDictionary;
            List<object> keys = new List<object>();
            List<object> values = new List<object>();
            foreach (object value_key in dictionary.Keys)
                keys.Add(value_key);
            foreach (object value_value in dictionary.Values)
                values.Add(value_value);

            result += "\n";
            for (int j = 0; j < keys.Count; j++)
            {
                object currentValue = values[j];
                if (Common.HasFields(ref currentValue))
                {
                    throw new Exception("Only built-in types are supported when serializing Dictionary<,>");
                    //This works for as long as the key is built-in
                    result += $"{Indent(indent + 1)}{keys[j]}: \n{Serialize(ref currentValue, indent + 2)}";
                }
                else
                    result += $"{Indent(indent + 1)}{keys[j]}: {Serialize(ref currentValue, indent + 1)}";
            }
            return result;
        }

        private static string SerializeClassOrStruct(ref object value, int indent)
        {
            string result = "";
            FieldInfo[] fields = value.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo fieldInfo = fields[i];
                if (fieldInfo.Attributes.HasFlag(FieldAttributes.NotSerialized))
                    continue;

                object fieldValue = fieldInfo.GetValue(value);

                if(Common.HasFields(ref fieldValue))
                    result += $"{Indent(indent)}{fieldInfo.Name}: \n{Serialize(ref fieldValue, indent + 1)}";
                else
                    result += $"{Indent(indent)}{fieldInfo.Name}: {Serialize(ref fieldValue, indent)}";
            }
            return result;
        }

        static string Indent(int indent)
        {
            string result = "";
            for (int i = 0; i < indent; i++)
                result += "\t";
            return result;
        }
    }

    public static class Deserializer
    {
        public static T Deserialize<T>(string tiny)
        {
            string[] lines = tiny.Split('\n');
            int indent = 0;

            //Determine Contexts
            Dictionary<string, List<string>> contexts = new Dictionary<string, List<string>>();
            for(int i = 0; i < lines.Length; i++)
            {
                if (!IsValidContextStart(ref lines[i], indent))
                    continue;

                List<string> contextLines = new List<string>() { lines[i] };
                for (int j = i + 1; j < lines.Length; j++)
                {
                    if (GetIndents(ref lines[j]) > indent)
                        contextLines.Add(lines[j]);
                    else
                        break;
                }
                if (contextLines.Count > 1)
                    contexts.Add(lines[i], contextLines);
            }

            //Call the constructor of the root object type
            Type type = typeof(T);
            object o = Activator.CreateInstance(type);

            //Populate
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (!IsValidContextStart(ref line, indent))
                    continue;

                string fieldName;
                string valueString;
                ConvertLineToNameAndValueString(ref line, out fieldName, out valueString);

                FieldInfo fieldInfo = type.GetField(fieldName);
                Type fieldType = fieldInfo.FieldType;

                if (valueString.Length > 0)
                    fieldInfo.SetValue(o, ConvertBuiltInType(ref valueString, fieldType));
                else if (contexts.ContainsKey(lines[i]))
                {
                    List<string> contextLines = contexts[lines[i]];

                    object value = null;
                    if (Common.Is1DArray(fieldType))
                        value = DeserializeArray(ref contextLines, fieldType);
                    else if (Common.IsList(fieldType))
                        value = DeserializeList(ref contextLines, fieldType);
                    else if(Common.IsMDArray(fieldType))
                        value = DeserializeMultiDimensionalArray(ref contextLines, fieldType);
                    else if(Common.IsDictionary(fieldType))
                        value = DeserializeDictionary(ref contextLines, fieldType);
                    else if(Common.HasFields(fieldType))
                        value = DeserializeClassOrStruct(ref contextLines, fieldType);

                    fieldInfo.SetValue(o, value);
                }
            }

            return (T)o;
        }

        private static Array DeserializeArray(ref List<string> contextLines, Type type)
        {
            Type elementType = type.GetElementType();
            Array array = null;

            if (IsBuiltInType(elementType))
            {
                array = Array.CreateInstance(elementType, contextLines.Count - 1);
                for (int i = 1; i < contextLines.Count; i++)
                {
                    string valueString = contextLines[i].Split('-')[1].Remove(0, 1);
                    array.SetValue(ConvertBuiltInType(ref valueString, elementType), i - 1);
                }
            }
            else
            {
                List<List<string>> jaggedContexts = GetJaggedContextLines(ref contextLines);
                array = Array.CreateInstance(elementType, jaggedContexts.Count);

                if (Common.Is1DArray(elementType) || Common.IsList(elementType))
                {
                    for (int i = 0; i < jaggedContexts.Count; i++)
                    {
                        List<string> jaggedContext = jaggedContexts[i];
                        object o = Common.Is1DArray(elementType) ? DeserializeArray(ref jaggedContext, elementType) : DeserializeList(ref jaggedContext, elementType);
                        array.SetValue(o, i);
                    }
                }
                else
                {
                    for (int i = 0; i < jaggedContexts.Count; i++)
                    {
                        List<string> subContextLines = jaggedContexts[i];
                        object value = null;
                        if (Common.IsMDArray(elementType))
                            value = DeserializeMultiDimensionalArray(ref subContextLines, elementType);
                        else if (Common.IsDictionary(elementType))
                            value = DeserializeDictionary(ref subContextLines, elementType);
                        else if (Common.HasFields(elementType))
                            value = DeserializeClassOrStruct(ref subContextLines, elementType);
                        array.SetValue(value, i);
                    }
                }
            }
            return array;
        }

        private static object DeserializeList(ref List<string> contextLines, Type type)
        {
            Type elementType = type.GetGenericArguments().Single();
            object list = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
            MethodInfo listAddMethod = list.GetType().GetMethod("Add");

            if (IsBuiltInType(elementType))
            {
                for (int i = 1; i < contextLines.Count; i++)
                {
                    string valueString = contextLines[i].Split('-')[1].Remove(0, 1);
                    listAddMethod.Invoke(list, new object[] { ConvertBuiltInType(ref valueString, elementType) });
                }
            }
            else
            {
                List<List<string>> jaggedContexts = GetJaggedContextLines(ref contextLines);
                list = Array.CreateInstance(elementType, jaggedContexts.Count);

                if (Common.Is1DArray(elementType) || Common.IsList(elementType))
                {
                    for (int i = 0; i < jaggedContexts.Count; i++)
                    {
                        List<string> jaggedContext = jaggedContexts[i];
                        object o = Common.Is1DArray(elementType) ? DeserializeArray(ref jaggedContext, elementType) : DeserializeList(ref jaggedContext, elementType);
                        listAddMethod.Invoke(list, new object[] { o });
                    }
                }
                else
                {
                    for (int i = 0; i < jaggedContexts.Count; i++)
                    {
                        List<string> subContextLines = jaggedContexts[i];
                        object value = null;
                        if (Common.IsMDArray(elementType))
                            value = DeserializeMultiDimensionalArray(ref subContextLines, elementType);
                        else if (Common.IsDictionary(elementType))
                            value = DeserializeDictionary(ref subContextLines, elementType);
                        else if (Common.HasFields(elementType))
                            value = DeserializeClassOrStruct(ref subContextLines, elementType);
                        listAddMethod.Invoke(list, new object[] { value });
                    }
                }
            }
            return list;
        }

        private static object DeserializeMultiDimensionalArray(ref List<string> contextLines, Type type)
        {
            return null;
        }

        private static object DeserializeDictionary(ref List<string> contextLines, Type type)
        {
            Type[] elementType = type.GetGenericArguments();
            object dict = Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(elementType));
            MethodInfo dictAddMethod = dict.GetType().GetMethod("Add");

            for (int i = 1; i < contextLines.Count; i++)
            {
                string line = contextLines[i];

                string[] keyValuePairString = new string[2];
                ConvertLineToNameAndValueString(ref line, out keyValuePairString[0], out keyValuePairString[1]);

                object[] keyValuePairObjects = new object[2];
                for (int j = 0; j < keyValuePairString.Length; j++)
                {
                    if (IsBuiltInType(elementType[j]))
                        keyValuePairObjects[j] = ConvertBuiltInType(ref keyValuePairString[j], elementType[j]);
                    else
                    {
                        return null;
                    }
                }

                dictAddMethod.Invoke(dict, keyValuePairObjects);
            }

            return dict;
        }

        private static object DeserializeClassOrStruct(ref List<string> contextLines, Type type)
        {
            int indent = GetIndents(contextLines[1]);
            object o = Activator.CreateInstance(type);

            int curContext = -1;
            for (int i = 1; i < contextLines.Count; i++)
            {
                string line = contextLines[i];

                if (!IsValidContextStart(ref line, indent))
                    continue;

                curContext++;

                string fieldName;
                string valueString;
                ConvertLineToNameAndValueString(ref line, out fieldName, out valueString);

                FieldInfo fieldInfo = type.GetField(fieldName);
                Type fieldType = fieldInfo.FieldType;

                if (valueString.Length > 0)
                    fieldInfo.SetValue(o, ConvertBuiltInType(ref valueString, fieldType));
                else
                {
                    List<string> subContextLines = GetSubContextLine(ref contextLines, i);

                    object value = null;
                    if (Common.Is1DArray(fieldType))
                        value = DeserializeArray(ref subContextLines, fieldType);
                    else if (Common.IsList(fieldType))
                        value = DeserializeList(ref subContextLines, fieldType);
                    else if (Common.IsMDArray(fieldType))
                        value = DeserializeMultiDimensionalArray(ref subContextLines, fieldType);
                    else if (Common.IsDictionary(fieldType))
                        value = DeserializeDictionary(ref subContextLines, fieldType);
                    else if (Common.HasFields(fieldType))
                        value = DeserializeClassOrStruct(ref subContextLines, fieldType);

                    fieldInfo.SetValue(o, value);
                }
            }

            return o;
        }

        private static int GetIndents(string line) => GetIndents(ref line);
        private static int GetIndents(ref string line)
        {
            int indents = 0;
            for (int j = 0; j < line.Length; j++)
            {
                if (line[j] == '\t')
                    indents++;
                else
                    break;
            }
            return indents;
        }

        private static List<List<string>> GetJaggedContextLines(ref List<string> contextLines)
        {
            List<List<string>> jaggedContexts = new List<List<string>>();
            int curContext = -1;
            for (int i = 1; i < contextLines.Count; i++)
            {
                string line = contextLines[i];
                int indents = GetIndents(ref line);
                if (indents == 1)
                {
                    curContext = jaggedContexts.Count;
                    jaggedContexts.Add(new List<string>() { line.Remove(0, 1) });
                }
                else if (indents > 1)
                    jaggedContexts[curContext].Add(line.Remove(0, 1));
            }
            return jaggedContexts;
        }

        private static List<string> GetSubContextLine(ref List<string> contextLines, int curIndex)
        {
            List<string> subContexts = new List<string>() { contextLines[curIndex].Remove(0, 1) };
            int curIndents = GetIndents(contextLines[curIndex]);

            if (curIndex + 1 < contextLines.Count)
            {
                for (int i = curIndex + 1; i < contextLines.Count; i++)
                {
                    string line = contextLines[i];
                    int indents = GetIndents(ref line);
                    if (indents > curIndents)
                        subContexts.Add(line.Remove(0, 1));
                    else
                        break;
                }
            }

            return subContexts;
        }

        private static bool IsBuiltInType(Type type)
        {
            bool result = false;
            result |= type == typeof(string);
            result |= type == typeof(bool);
            result |= type == typeof(byte);
            result |= type == typeof(sbyte);
            result |= type == typeof(char);
            result |= type == typeof(decimal);
            result |= type == typeof(double);
            result |= type == typeof(float);
            result |= type == typeof(int);
            result |= type == typeof(uint);
            result |= type == typeof(long);
            result |= type == typeof(ulong);
            result |= type == typeof(short);
            result |= type == typeof(ushort);
            return result;
        }

        private static T ConvertBuiltInType<T>(ref string valueString) => (T)ConvertBuiltInType(ref valueString, typeof(T));
        private static object ConvertBuiltInType(ref string valueString, Type type)
        {
            if (valueString == "~" || valueString == "null")
                return null;

            object value = null;
            try
            {
                if (type == typeof(string))
                    value = valueString;
                else if (type == typeof(bool))
                    value = ConvertStringToBoolean(ref valueString);
                else if (type == typeof(byte))
                    value = byte.Parse(valueString);
                else if (type == typeof(sbyte))
                    value = sbyte.Parse(valueString);
                else if (type == typeof(char))
                    value = valueString != null && valueString.Length > 0 ? valueString[0] : '\0';
                else if (type == typeof(decimal))
                    value = decimal.Parse(valueString);
                else if (type == typeof(double))
                    value = double.Parse(valueString);
                else if (type == typeof(float))
                    value = float.Parse(valueString);
                else if (type == typeof(int))
                    value = int.Parse(valueString);
                else if (type == typeof(uint))
                    value = uint.Parse(valueString);
                else if (type == typeof(long))
                    value = long.Parse(valueString);
                else if (type == typeof(ulong))
                    value = ulong.Parse(valueString);
                else if (type == typeof(short))
                    value = short.Parse(valueString);
                else if (type == typeof(ushort))
                    value = ushort.Parse(valueString);
            }
            catch(Exception e)
            {
                throw new Exception("Tiny Parsing Failed", e);
            }

            return value;
        }

        private static bool? ConvertStringToBoolean(ref string text)
        {
            if(text == null)
                return null;

            if (text.ToLower() == "true")
                return true;
            else if (text.ToLower() == "false")
                return false;

            return null;
        }

        private static bool IsValidContextStart(ref string text, int indent) => text.Length > indent && GetIndents(ref text) == indent && text.Contains(":");
        private static bool IsValidElementStart(ref string text, int indent) => text.Length > indent && GetIndents(ref text) == indent && text.Contains("-");

        private static void ConvertLineToNameAndValueString(ref string line, out string name, out string value)
        {
            string[] nameAndValue = line.Split(':');
            name = nameAndValue[0];
            value = nameAndValue[1];

            while(name.Length > 0 && name[0] == '\t')
                name = name.Remove(0, 1);

            if (value.Length > 0 && value[0] == ' ')
                value = value.Remove(0, 1);
        }
    }
}