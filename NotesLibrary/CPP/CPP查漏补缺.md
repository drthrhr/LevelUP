> ——用于记录一些在复习CPP的过程中，发现自己还不是很熟悉的一些知识点，以作查漏补缺之用。

# 类型转换

## 隐式类型转换

隐式类型转换通常指不同类型的数据互相一起参与算术运算，或者赋值时，编译器并不报错，而是替我们将小类型的变量值转换成大类型的变量值从而正常运算的过程。

比如，

```cpp
double i = 100/3;//33
```

上面的i最终的值是33。而不是33.3333333....

这是因为100和3这两个字面值常量都会被解释成int类型。

两个int类型做除法的结果仍然是int类型。

当int类型的值赋值给double类型的i时，编译器认为这没有什么不合理的，就把int值33转换为double类型值33.0存到了变量i内。这就是隐式类型转换。

## 显式类型转换

隐式类型转换是比较安全的，返回来就正好不那么安全了。

比如，把double类型的值赋值给int类型的变量。

```cpp
int i = 100 / 3.0;
```

这里10是整数，3.0是浮点数（double）。在C++中，整数和浮点数做除法时，结果会提升为浮点数，所以10 / 3.0的结果是3.33333（类型为double）。而double类型8字节，int类型4字节，很容易存不下。

这时候编译器会给你一个警告：

```text
main.cpp(8): warning C4244: “初始化”: 从“double”转换到“int”，可能丢失数据
```

![img](M:\LevelUP\NotesLibrary\CPP\assets\v2-6b5699fe92df6ae00b11e2fdcd224322_1440w.jpg)

上面的警告说：main.cpp第8行存在double转到int类型。

那应该怎么办呢？

可以使用C++的显示类型转换操作符来实现。

```cpp
int i = static_cast<int>( 100 / 3.0);
```

这样明确的告诉编译器，这是我希望发生的，不用警告我，我心里有数。

![img](M:\LevelUP\NotesLibrary\CPP\assets\v2-468fd5882d912fea6041350264fa7dea_1440w.jpg)

此时就看不到编译器报警告了。

`static_cast<目标类型>(表达式)`：用于安全地进行类型转换。比如：浮点数转整数（如本例）、整数转浮点数、父类指针转子类指针（有继承关系时）。注意static_cast 不会做运行时类型检查，转换时只做语法检查；且转换时可能会丢失精度（如本例的小数部分被截断）。

# 命名空间

[C++——命名空间（namespace）_c++ namespace-CSDN博客](https://blog.csdn.net/m0_49687898/article/details/131350690)

命名空间用于解决变量的命名冲突问题。

## 命名空间的定义

定义命名空间，需要使用到namespace关键字，后面跟命名空间的名字，然后接一对{ }即可，{}中即为命名空间的成员。

1. 命名空间可以定义变量，函数，类型

   ```c++
   //1. 普通的命名空间
   namespace N1 // N1为命名空间的名称
   {
   	// 命名空间中的内容，既可以定义变量，也可以定义函数，也可以定义类型
   	int a; //变量
   	int Add(int left, int right) //函数
   	{
   		return left + right;
   	}
       struct ListNode //类型
       {
           int val;
           struct ListNode* next;
       }
    
   }
   ```

2. 命名空间可以嵌套

   ```c++
   //2. 命名空间可以嵌套
   namespace N2
   {
   	int a;
   	int b;
   	int Add(int left, int right)
   	{
   		return left + right;
   	}
   	namespace N3
   	{
   		int c;
   		int d;
   		int Sub(int left, int right)
   		{
   			return left - right;
   		}
   	}
   }
   ```

3. 同一个工程中允许存在多个相同名称的命名空间,编译器最后会合成同一个命名空间中

   ```c++
   //3. 同一个工程中允许存在多个相同名称的命名空间,编译器最后会合成同一个命名空间中。
   namespace N1
   {
   	int Mul(int left, int right)
   	{
   		return left * right;
   	}
   }
   ```

## 命名空间的使用

在C语言中，存在局部优先规则，如下：

```c++
int a = 0; //全局域
int main()
{
	int a = 1; //局部域
	printf("%d\n", a); // 打印结果：1。局部优先，访问的是局部域中的a，即1
	return 0;
}
```

*可以使用域作用限定符 ( :: )来改变程序所访问的域。如：*

```c++
int a = 0; //全局域
int main()
{
	int a = 1; //局部域
	printf("%d\n", ::a); // 打印结果：0。域作用限定符::前面是空白，那么访问的就是全局域中的a，即0
	return 0;
}
```

### 使用域作用限定符::访问命名空间中的成员

如：

```c++
namespace ret
{
	struct ListNode
	{
		int val;
		struct ListNode* next;
	};
}
 
namespace tmp
{
	struct ListNode
	{
		int val;
		struct ListNode* next;
	};
	struct QueueNode
	{
		int val;
		struct QueueNode* next;
	};
}

int main()
{
	struct ret::ListNode* n1 = NULL;
	struct tmp::ListNode* n2 = NULL;
	return 0;
}
```

或对于嵌套的命名空间，可以这样进行访问：

![img](M:\LevelUP\NotesLibrary\CPP\assets\3ffa22153e7a876bc94bcbfbbaf1578c.png)

```c++
int main()
{
	struct tx::List::Node* n1; //访问List.h文件中的Node
	struct tx::Queue::Node* n2;//访问Queue.h文件中的Node
}
```

