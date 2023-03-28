# TINY
This is not YAML

As of v0.8:
MultiDimensional Array's not supported.
Dictionary supports built-in value types and string.

Output from TINYSample project:
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
