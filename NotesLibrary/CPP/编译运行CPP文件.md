> ——编译运行cpp文件的一些理解与方法

# 使用VSCode

## 单个文件的运行与调试

环境搭建可参照[VS Code 配置 C/C++ 编程运行环境（保姆级教程）_vscode配置c++环境-CSDN博客](https://blog.csdn.net/qq_42417071/article/details/137438374)

**其实本质上**，就是利用了VSCode中的C语言插件。在通过某种方式安装gcc编译器并配置环境变量之后，这个插件就能自动识别到所安装的gcc编译器路径，然后根据要运行的文件等信息，生成配置文件tasks.json，然后基于其中的内容自动拼接命令，调用所指定的gcc编译器，编译链接.cpp源代码并生成可执行的.exe文件，再自动在VSCode终端中运行它。**整体上确实就是VSCode插件的思路，跟我最开始写支持交叉编译功能的VSCode插件的整体思路很像，就是获取配置，生成配置文件，再根据配置文件自动拼接命令并执行。**

还有就是要注意所采用编译调试工具的对应关系。

- 比如使用**MSYS2终端（相当于在Windows上运行的一个类似于Linux的终端环境）**所下载的**MinGW-w64工具链**，那么就是用**gcc**或者**g++**编译器来进行编译，然后用**gdb**工具来进行调试，在VSCode中选择的时候要选对，要选后面括号里是**GDB/LLDB**的
  - 如果编译的是.c文件，则可以使用gcc编译器
  - 如果编译的是.cpp文件，则只能用g++编译器，而不能用gcc。gcc是指支持c语言的，确实也是很自然而然地不能向未来兼容c++。
- 如果使用**MSVC**编译器，那在VSCode中选的时候就需要选后面括号里是**Windows**的

~~单个文件的运行与调试比较简单，只通过所生成的tasks.json就可以实现。~~

单个文件进行运行或调试的信息，被储存在VSCode当前工作区目录（也就是所打开的文件夹）的.vscode文件夹中的tasks.json文件中。

## 多个文件的运行与调试

通过修改所自动生成的tasks.json文件，可以指使VSCode的C语言插件拼接出能够链接多个文件的简单命令。比如当前工作区文件夹为test，其目录结构为：

```
test
  |--caculate.c
  |--max.c
  |--max.h
```

其中各文件内容如下：

- caculate.c：

  ```c
  #include <stdio.h>
  #include "max.h"
  
  int main()
  {
      int a = 10;
      int b = 20;
      int c = findMaxNum(a, b);
      printf("%d\n", c);
      return 0;
  }
  ```

- max.c：

  ```c
  #include "max.h"
  
  int findMaxNum(int num1, int num2)
  {
      return num1 > num2 ? num1 : num2;
  }
  ```

- max.h：

  ```c
  #ifndef __MAX_H__
  #define __MAX_H__
  #include <stdio.h>
  
  int findMaxNum(int num1, int num2);
  
  #endif // __MAX_H__
  ```

则可在caculate.c中点击右上角的调试选项，会自动生成tasks.json。此时文件目录结构变为：

```
test
  |--.vscode
    |--tasks.json
  |--caculate.c
  |--max.c
  |--max.h
```

所自动生成的tasks.json内容为：

```json
{
    "tasks": [
        {
            "type": "cppbuild",
            "label": "C/C++: gcc.exe 生成活动文件",
            "command": "M:\\MProgramFiles\\msys64\\ucrt64\\bin\\gcc.exe",
            "args": [
                "-fdiagnostics-color=always",
                "-g",
                "${file}",
                "-o",
                "${fileDirname}\\${fileBasenameNoExtension}.exe"
            ],
            "options": {
                "cwd": "${fileDirname}"
            },
            "problemMatcher": [
                "$gcc"
            ],
            "group": {
                "kind": "build",
                "isDefault": true
            },
            "detail": "调试器生成的任务。"
        }
    ],
    "version": "2.0.0"
}
```

其中args数组内就保存了要附加在命令之后的各项参数。因此可以通过修改这些参数，实现把所有.c文件全部编译，最后生成目标指定为一个名为program.exe的文件。可将tasks.json改为如下内容：

```json
{
    "tasks": [
        {
            "type": "cppbuild",
            "label": "C/C++: gcc.exe 生成活动文件",
            "command": "M:\\MProgramFiles\\msys64\\ucrt64\\bin\\gcc.exe",
            "args": [
                "-fdiagnostics-color=always",
                "-g",
                // "${file}",
                "*.c",
                "-o",
                // "${fileDirname}\\${fileBasenameNoExtension}.exe"
                "${fileDirname}\\program.exe"
            ],
            "options": {
                "cwd": "${fileDirname}"
            },
            "problemMatcher": [
                "$gcc"
            ],
            "group": {
                "kind": "build",
                "isDefault": true
            },
            "detail": "调试器生成的任务。"
        }
    ],
    "version": "2.0.0"
}
```

之后再点击调试，就可以成功编译并断点调试了。

*这种好像是比较简单的项目结构的办法。可能复杂一些的就得写左边“运行和调试”一栏中的具体配置，launch.json了*