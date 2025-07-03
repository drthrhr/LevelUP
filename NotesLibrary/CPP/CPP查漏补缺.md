> ——用于记录一些在复习CPP的过程中，发现自己还不是很熟悉的一些知识点，以作查漏补缺之用。

# 类型转换

## 1 隐式类型转换

隐式类型转换通常指不同类型的数据互相一起参与算术运算，或者赋值时，编译器并不报错，而是替我们将小类型的变量值转换成大类型的变量值从而正常运算的过程。

比如，

```cpp
double i = 100/3;//33
```

上面的i最终的值是33。而不是33.3333333....

这是因为100和3这两个字面值常量都会被解释成int类型。

两个int类型做除法的结果仍然是int类型。

当int类型的值赋值给double类型的i时，编译器认为这没有什么不合理的，就把int值33转换为double类型值33.0存到了变量i内。这就是隐式类型转换。

## 2 显式类型转换

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

# 模板

在 C++11 和之前的版本中模板主要分成两种，一种是函数模板，另一种是类模板。

函数模板是通过模板定义关键字 **`template`** 和泛型参数来定义的，这些泛型参数可以在用来修饰函数的参数列表、返回类型或函数体中使用。

类模板是通过模板关键字 **`template`** 和泛型参数来定义的，这些参数可以在类的成员变量、成员函数或类体中使用。

## 定义模板与模板参数

模板参数包括类型模板参数和非类型模板参数。

- 类型模板参数就是顾名思义，可以适配不同类型的，作为模板的参数；
- 非类型模板参数就是具体类型的参数，不是作为模板适配不同类型的。

使用关键字 **`template`** 来定义模板，使用关键字**`typename`**来定义类型模板参数，使用对应类型的名称来定义非类型模板参数。如：

```cpp
template<typename T, int SIZE>
```

## 函数模板

即使用模板来定义函数。C++ 函数模板允许编写可以适用于不同类型的函数。如：

```cpp
#include<iostream>
template<typename T>
T max(T a,T b){ return(a > b)?a : b;}
int main()
{
    int x  = 5,y  = 10;
    float p  = 3.14, q  = 2.7;
    
    //max()函数被实例化成int max(int a,int b)的形式
    std::cout<< max(x,y)<< std::endl;   //输出10
    
    //max()函数被实例化成int max(float a,float b)的形式
    std::cout<< max(p,q)<< std::endl;   //输出3.14
    
    return 0;
}
```

## 类模板

即使用模板来定义类。类模板允许编写可以适用于不同数据类型的类。如：

```cpp
#include <iostream>
#include <string>

template <typename T>
class Stack
{
    private:
        T *data;
        int size;
        int capacity;

    public:
        // 构造函数，初始化容量、大小，分配内存
        Stack(int cap) : capacity(cap), size(0)
        {
            data = new T[capacity];
        }
        // 析构函数，释放动态分配的内存，防止内存泄漏
        ~Stack()
        {
            delete[] data;
        }
        void push(T element)
        {
            if (size < capacity)
            {
                data[size++] = element;
            }
        }
        T pop()
        {
            if (size == 0)
            {
                // 这里根据 T 类型合理返回“空”值，对于基本类型可返回默认构造值，复杂类型可抛异常等，这里简单处理
                return T();
            }
            return data[--size];
        }
        bool isEmpty()
        {
            return size == 0;
        }
    };

int main()
{
    // 实例化成整型数的 Stack
    Stack<int> intStack(5);
    intStack.push(1);
    intStack.push(2);
    intStack.push(3);
    std::cout << intStack.pop() << std::endl; // 输出 3
    // 实例化成 std::string 的 Stack
    Stack<std::string> stringStack(5);
    stringStack.push("hello");
    stringStack.push("world");
    std::cout << stringStack.pop() << std::endl; // 输出 world
    return 0;
}
```



# 命名空间

[C++——命名空间（namespace）_c++ namespace-CSDN博客](https://blog.csdn.net/m0_49687898/article/details/131350690)

命名空间用于解决变量的命名冲突问题。

## 1 命名空间的定义

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

## 2 命名空间的使用

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

### 2.1 使用域作用限定符::访问命名空间中的成员

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

### 2.2 使用`using namespace`将命名空间全部展开

如：

```c++
using namespace tx;	//将命名空间全部展开，即把tx这个命名空间定义的东西放出来

int main()
{
	struct List::Node* n1; //访问List.h文件中的Node
	struct Queue::Node* n2;//访问Queue.h文件中的Node
}
```

也可以再拆一层，如下：

```c++
//展开时要注意tx和List的顺序不能颠倒
using namespace tx;
using namespace List;

int main()
{
	struct Node* n1; //访问List.h文件中的Node
	struct Queue::Node* n2;//访问Queue.h文件中的Node
}
```

这种访问方式是可以达到简化效果，但是也会存在一定风险：命名空间全部释放又重新回到命名冲突。如：

![img](M:\LevelUP\NotesLibrary\CPP\assets\0f609799d6cd32a1f7e7469a2ad66671.png)

### 2.3 使用`using`将命名空间中成员展开

如针对上述代码，可以只把n1中的f放出来：

```c++
namespace n1
{
	int f = 0;
	int rand = 0;
}
using n1::f;

int main()
{
	f += 2;
	printf("%d\n", f);
	n1::rand += 2;
	printf("%d\n", n1::rand);
}
```

------

### 2.4 （常用的示例）

以C++的标准库命名空间std为例，使用上述三种方法的示例分别为：

1. 使用域作用限定符::访问命名空间中的成员

   ```c++
   #include<iostream>
   using namespace std; //std 是封C++库的命名空间
   int main()
   {
   	cout << "hello world" << endl; // hello world
   	return 0;
   }
   ```

2. 使用`using namespace`将命名空间全部展开

   ```c++
   #include<iostream>
   int main()
   {
   	std::cout << "hello world" << std::endl;
   	return 0;
   }
   ```

3. 使用`using`将命名空间中成员展开

   ```c++
   #include<iostream>
   using std::cout;
   int main()
   {
   	cout << "hello world" << std::endl;
   	return 0;
   }
   ```

# 引用

引用可以理解为变量的别名，创建引用的时候并没有创建新的变量，引用名和原变量名指向的是同一块内存空间，所对应的变量是同一个东西。

## 1 语法&简单用法

假设 `type` 是一种类型，`type`类型的引用类型就是 `type&`， 也就是在类型的后面加一个&符号。

```cpp
type a;//创建变量 a
type& b = a; //创建引用类型的变量b, b是a的引用
```

引用类型的变量和原来的变量对应的是一个东西。修改引用就是修改原来的变量

```cpp
#include <iostream>
using namespace std;

int main(void)
{
	int a = 1;
	int& b = a;//创建别名（引用）
	int c = a;//创建副本（拷贝）

	b = 222;//修改 a 
	c = 333;//修改 c

	cout << "a=" << a << endl;	// a=222
	cout << "b=" << b << endl;	// b=222
	cout << "c=" << c << endl;	// c=333

	return 0;
}
```

引用主要在下面两种情况下使用：

1. [向函数传递参数](https://zhuanlan.zhihu.com/p/659450002)的时候传递引用比较方便；
2.  [操作符重载](https://zhuanlan.zhihu.com/p/262508774)的时候返回引用可以实现连续调用；

# 标准输入输出

输出好理解，就是cout呗，可以参考[1.2.1 输出变量(cout与endl换行显示) - 知乎](https://zhuanlan.zhihu.com/p/659417617)

这里的查漏补缺主要是输入，有关不同的输入方式与换行符的读取。

## 1 标准输入

### 1.1 cin

`std::cin` 就能从输入缓冲区中获得到非空的一行。注意：

- 空行、空格、tab等均不会被获取到。
- 不会删除输入缓冲区中的空行（也就是说比如输入完【111\n】之后，用cin只会获取到`111`，还会在输入缓冲区中留下一个`\n`）
- 可以连着用 `>>` 赋值给多个变量，对应地，程序也会阻塞等待对应数量的非空行输入完毕

比如：

```cpp
#include <iostream>
#include <string>
using namespace std;

int main()
{
	string line1, line2;

	/* 输入： \n\n  \n111 \n\n   \n  \t 222\n （\n代表回车；\t代表tab键） */
	cin >> line1 >> line2;	// line1 = "111", line2 = "222"
	return 0;
}
```

### 1.2 getline

`std::getline` 用于从输入流（如`std::cin`或文件流）中读取一行文本，并将其存储到一个`std::string`对象中。

注意点：

- 可以读取包含空格、tab的整行输入
- 在输入缓冲区中读取，遇到\n（即默认的分隔符字符）就会停止读取，并且会把所遇到的那个\n从输入缓冲区中删掉

函数原型与说明：

- ```cpp
  istream& getline(istream& is, string& str);
  istream& getline(istream& is, string& str, char delim);
  ```

- `is`: 输入流对象（如 `std::cin` 或文件流）。
- `str`: 用于存储读取到的字符串的 `std::string` 对象。
- `delim` (可选): 分隔符字符，默认为换行符（`'\n'`）。当遇到该字符时，读取停止。
- 其返回值是一个输入流对象 `is`，支持链式调用。

比如：

```cpp
#include <iostream>
#include <string>
using namespace std;

int main()
{
	string line1, line2;

	/* 输入：\t  \t   111  \t\n\n222（\t代表tab；\n代表回车）*/
	getline(cin, line1);	// line1 = "\t  \t   111  \t"
	getline(cin, line2);	// line2 = ""
	return 0;
}
```

### 1.3 混用cin与getline

由于cin、getline的上述特性，因此在混用cin与getline时要注意可能残存在输入缓冲区中的\n。

比如：

```cpp
#include <iostream>
#include <string>
using namespace std;

int main()
{
	cout << "Hello CMake." << endl;
	string line1, line2;

	/* 输入:111\n222 （\n代表回车） */
	getline(cin, line1);	// line1 = "111"
	cin >> line2;	// line2 = "222"

	return 0;
}
```

```cpp
#include <iostream>
#include <string>
using namespace std;

int main()
{
	cout << "Hello CMake." << endl;

	string line1, line2;

	/* 输入:111\n222 （\n代表回车）（实际上输入完111\n就执行完这两条语句了） */
	cin >> line1;	// line1 = "111"
	getline(cin, line2);	// line2 = ""

	return 0;
}
```

为避免cin后残存在缓冲区的\n对后续getline造成影响，可用 `std::cin.ignore()` 来清除缓冲区，再用getline。

# 内存分配

C++内存分配有栈、堆和静态存储区三种方式。栈自动管理，适用于局部变量；堆手动管理，使用new和delete；静态存储区适用于全局变量，具有整个程序生命周期。

## 栈方式

栈上的内存是自动分配和释放的，由编译器管理。局部变量和函数调用信息存储在栈上，栈指针自动上下移动。栈变量的生命周期与其所在的作用域相同，当变量离开作用域时，它们自动被销毁。如：

```cpp
#include <iostream>

void stackExample() {
    int stackVar = 10; // 在栈上分配变量
    std::cout << "Stack Variable: " << stackVar << std::endl;
    // stackVar 在函数结束时自动销毁
}

int main() {
    stackExample();
    return 0;
}
```

## 堆方式

堆上的内存由程序员手动分配和释放，使用 new 和 delete。堆变量的生命周期由程序员手动控制，需要显式释放内存，否则可能导致内存泄漏。如：

```cpp
new int{456};//堆变量没有名字，只有地址
```

堆变量没有名字，但是new会把堆变量的地址返回给我们。这样我们可以通过**地址变量**来存放这个地址：

```cpp
#include <iostream>

int main() {
    int* heapVar = new int(20); // 在堆上分配变量
    std::cout << "Heap Variable: " << *heapVar << std::endl;
    // 需要手动释放堆上的内存
    delete heapVar;
    return 0;
}
```

地址变量可以被赋值，即通过new方式申请内存之后，所得到的内存地址放在一个地址变量中；这个地址变量存放了一个堆地址，而这个地址变量本身也需要占用一个特定地址开始的一段存储空间，即栈中的存储空间。一个示例代码以及对应的存储空间示意图如下：

- ```cpp
  int* a = new int;
  int* b = a;//a 和 b 存储的内容相同，都是堆上的那个无名变量的地址
  ```

- ![img](M:\LevelUP\NotesLibrary\CPP\assets\v2-3f78c6457077b7131ddaa85bfb0c44b1_1440w.jpg)

## 静态存储区

静态变量在程序运行前分配，程序结束时释放。静态变量和全局变量存储在静态存储区，具有整个程序的生命周期。如：

```cpp
#include <iostream>

int staticVar = 30; // 静态变量在静态存储区

int main() {
    std::cout << "Static Variable: " << staticVar << std::endl;
    return 0;
}
```

## 有关内存泄露

使用 `new` 方式申请的堆变量，为便于使用会把它的地址赋给一个栈变量，作为地址变量；而栈变量是有作用域和生命周期的，如果在其整个生命周期内都没有销毁该地址变量中所储存地址所指向的堆变量所占用的内存空间，那么当这个存储了堆变量地址的栈变量生命周期结束，它被销毁之后，就再也没有找到其存储地址的堆变量的机会了，这个堆变量就会一直占用着那一块内存空间，从而导致内存泄漏。

所以对于使用 `new` 方式申请的堆变量，在使用完毕后，在其地址变量的生命周期结束前，应该及时使用 `delete` 方式予以销毁。如下所示：

```cpp
void g(void){
  int* a = new int;
  int b;
  delete a;//释放a指向的无名变量
}
void f(void){
  int x;
  int y;
  g();
}
int main(void){
   f();
}
```

语句`delete a;` 执行之后，这部分内存就被操作系统收回了，当前程序如果访问，就属于越权。越权是不允许的，操作系统会把当前程序杀掉。调试环境下会抛异常。

使用delete释放的内存，其实是把内存空间归还给了操作系统，这样操作系统就可以在合适的时机，再次把那块内存分配给其他程序申请使用。

## 有关 `&` 与 `*` 

这俩符号的含义，可以区分为在两大种情况下，各有不同：

### 在定义的时候

定义或者声明变量时，

-  `&` 指引用某个变量，可以理解对一个东西“取别名”；既然要取别名那肯定就得指定是对谁，所以在定义引用的时候必须初始化。如：

  ```cpp
  void fun(int& a) {}	//声明参数是引用类型
  
  int a;
  int& ra = a ;	//定义引用类型的变量，必须初始化
  ```

-  `*` 代表定义一个指针变量（或者叫地址变量），其存放的值为某个东西的地址。如：

  ```cpp
  void fun(int* a) {}	//声明参数是指针类型
  
  int* p = nullptr, *q;	//定义指针类型的变量
  ```

### 在使用的时候

与定义或者声明的场景不同，在具体使用的时候，即只有这个符号，把它作为运算符用到某个变量身上的时候，

- `&` 指取地址，取得某个变量在内存中所占连续存储空间的起始地址。如

  ```cpp
  void fun(int* p)
  {
      //下面的 4 种修改堆数组的写法是一样的
      *p = 4;//修改堆数组的第1个元素；等价于 *(p+0) ，等价于 p[0]
      *(p + 1) = 5;//修改堆数组的第2个元素
      p[2] = 6;//修改堆数组的第3个元素
  
      p = p + 3;//指针移动跳过 3 个整型变量
      *p = 7;//此时p指向对数组的最后一个元素（第4个元素）
  }
  
  int main()
  {
      //开辟包含4个元素的堆数组，每个元素的值分别是0 1 2 3
      int* arr = new int[4] {0, 1, 2, 3};
  
      //输出数组内容	arr : 0, 1, 2, 3
      cout << "arr : " << arr[0] << ", " << arr[1] << ", " << arr[2] << ", " << arr[3] << endl;
  
      int* p = arr;
      fun(p);//堆数组的第一个元素的地址，传递给函数fun
  
      //输出数组内容	arr : 4, 5, 6, 7
      cout << "arr : " << arr[0] << ", " << arr[1] << ", " << arr[2] << ", " << arr[3] << endl;
  
      delete[] arr;
      return 0;
  }
  ```

- `*` 指解引用，或者可以理解为“取内容”，和指针变量连着用，就相当于获得了以该指针所指地址为起始地址，在内存中所存储的那个变量本身。如

  ```cpp
  int* a = new int{ 123 };		//创建一个栈上的地址变量 a，a 存储了堆上一个无名变量的地址。
  *a = 2;							//获得a所存地址对应的变量，将其赋值为2
  cout << "a = " << *a << endl;	// a = 2
  ```

### 一个混起来用的例子

```cpp
void fun(int& a)//这里的 & 表示参数a是引用类型
{
    a = 4;
}
void fun2(int* p)
{
    *p = 6;
}

int main()
{
    int n = 2;
    cout << "n=" << n << endl;	// n=2
 
    int& nn = n;//这里的 & 是引用类型， nn 是 n 的别名
    nn = 3;
    cout << "n=" << n << endl;	// n=3
 
    fun(n);
    cout << "n=" << n << endl;	// n=4
    
    int* p = &n;//这里的 & 是取地址运算符，&n 是 n 的地址
    *p = 5;
    cout << "n=" << n << endl;	// n=5
 
    fun2(&n);//这里的 & 是取地址运算符，&n 是 n 的地址
    cout << "n=" << n << endl;	// n=6
    
    return 0;
}
```
