using System;
using System.IO;
using System.Collections.Generic;

namespace TinySample
{
    public class MySimpleClass
    {
        public byte MyByte;
        public sbyte MySByte;
        public short MyShort;
        public ushort MyUShort;
        public uint MyUInt;
        public long MyLong;
        //public half MyHalf;
        public double MyDouble;
    }

    public struct Vector2
    {
        public int x;
        public int y;
    }

    public class MyClass
    {
        //Things to serialize:
        public bool MyBoolean;
        public string MyString;
        public int MyInteger;
        public float MyFloat;
        public char MyChar;
        public int[] MySingleDimensionArray;
        public List<int> MyList;
        //public int[,] MyTwoDimensionArray; //Not Supported Yet
        public int[][] MyJaggedDimensionArray;
        public Dictionary<int, string> MyDictionary;
        public MyStruct MyStruct;
        public MySimpleClass MySimpleClass;

        //Things not to serialize:
        [System.NonSerialized] public int MyNonSerializedPublicInteger;
        public static int MyStaticInteger;
        private int MyPrivateInteger;
        public MySimpleClass MyProperty => MySimpleClass;
    }

    public struct MyStruct
    {
        //public Dictionary<string, List<string>> MyDictionaryOfLists; //Not Supported Yet
        public Vector2[] MyArrayOfVectors;
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            MySimpleClass mySimpleClass = new MySimpleClass();
            mySimpleClass.MyByte = byte.MaxValue;
            mySimpleClass.MySByte = sbyte.MinValue;
            mySimpleClass.MyShort = short.MaxValue;
            mySimpleClass.MyUShort = ushort.MinValue;
            mySimpleClass.MyUInt = uint.MaxValue;
            mySimpleClass.MyLong = long.MaxValue;
            mySimpleClass.MyDouble = Math.PI;

            MyClass myClass = new MyClass();
            myClass.MyBoolean = true;
            myClass.MyString = "Lorem Ipsum...";
            myClass.MyInteger = 123;
            myClass.MyFloat = 1.5f;
            myClass.MyChar = 'A';
            myClass.MySingleDimensionArray = new int[] { 1, 2, 3 };
            myClass.MyList = new List<int>() { 2, 1 };
            //myClass.MyTwoDimensionArray = new int[,] { { 1, 2, 3 }, { 3, 4, 5 } };
            myClass.MyJaggedDimensionArray = new int[][] { new int[] { 1, 2, 3 }, new int[] { 2, 1 } };
            myClass.MyDictionary = new Dictionary<int, string>() { { 1, "One" }, { 2, "Two" } };
            myClass.MyStruct = new MyStruct()
            {
                /*
                MyDictionaryOfLists = new Dictionary<string, List<string>>(){
                    {"shapes", new List<string>() { "box", "sphere", "cylinder" } },
                    { "cars", new List<string>() { "ferrari", "mclaren", "williams" } }
                },
                */
                MyArrayOfVectors = new Vector2[] { new Vector2() { x = 1, y = 2 }, new Vector2() { x = 3, y = 4 }, }
            };
            myClass.MySimpleClass = mySimpleClass;

            string myClassTiny = Tiny.Serializer.Serialize(myClass);
            File.WriteAllText("myClass.tiny", myClassTiny);

            MyClass myDeserializedClass = Tiny.Deserializer.Deserialize<MyClass>(myClassTiny);

            string myDesierializedClassTiny = Tiny.Serializer.Serialize(myDeserializedClass);
            File.WriteAllText("myDeserializedClass.tiny", myDesierializedClassTiny);

            Console.Read();
        }
    }
}