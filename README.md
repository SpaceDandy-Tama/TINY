# TINY

**T**his **I**s **N**ot **Y**AML.

## Version Information

### Current Version: v0.8.3
- MultiDimensional Arrays are not supported.
- Dictionaries support built-in value types and strings.

### Changelog
- **v0.8.3**: Addresses a bug for deserializing tiny files with fields missing in the target object class.
- **v0.8.2**: Adds enum deserialization.
- **v0.8.1**: Addresses a bug when deserializing strings that contain `':'`.
- **v0.8**: Initial Version.

### Projected Future Versions
- **v1.0**: Will add MultiDimensional Array support.
- **v0.9**: Will add Dictionary custom value type support.

## Output from TINYSample Project
```
MyBoolean: True
MyString: Lorem Ipsum...
MyInteger: 123
MyFloat: 1.5
MyChar: A
MySingleDimensionArray: 
	- 1
	- 2
	- 3
MyList: 
	- 2
	- 1
MyJaggedDimensionArray: 
	- 
		- 1
		- 2
		- 3
	- 
		- 2
		- 1
MyDictionary: 
	1: One
	2: Two
MyStruct: 
	MyArrayOfVectors: 
		- 
			x: 1
			y: 2
		- 
			x: 3
			y: 4
MySimpleClass: 
	MyByte: 255
	MySByte: -128
	MyShort: 32767
	MyUShort: 0
	MyUInt: 4294967295
	MyLong: 9223372036854775807
	MyDouble: 3.14159265358979
```

## How to Use

```csharp
string tiny = Tiny.Serializer.Serialize(myClass);
MyClass myDeserializedClass = Tiny.Deserializer.Deserialize<MyClass>(tiny);
```
