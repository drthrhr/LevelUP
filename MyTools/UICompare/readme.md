# UI对比：公共代码模板生成

用于生成UI对比项目的公共代码。

## 基本用法

1. 在脚本前几行所定义的常量中，指定需要处理的Excel文件、sheet列表、分词词库、输出目录等信息。
2. 提供包含了用例编号的表格文件。脚本会扫描所指定表格中 "编号" 列，获取用例编号信息。
3. 运行脚本，可在指定位置实现：
   - 生成包含所提供用例信息的测试脚本文件
   - 生成与各用例信息所对应的各测试页面文件
   - 更新 pages.json 文件，添加对应各用例的内容
   - 更新 List.test.ets 文件，添加各用例对应测试套件的内容
4. 若分词出现错误，可手动修改词库，添加自定义名词：
   1. 进入词库路径
   2. 解压 `wordninja_words.txt.gz` 
   3. 修改解压出的 .txt 文件，将自定义名词加在最前面
   4. 用 7Zip 重新压缩为 gzip 格式文件，替换原有的 `wordninja_words.txt.gz`



## 未实现的功能

有些功能感觉实现起来也要半天研究，很麻烦，并且就算真的实现了也收益不高，自己不咋经常用，或者能省的时间很少。记录一下，以后真闲了的话可以接着完善。

- [x] 加入对一次性处理属于多个 suite 的数据的支持。现阶段用的时候粘到表里的数据都是同属于一个suite的，代码里写的就是不断更新当前的`suiteName`的值，相当于拿最后一个用例的 `suiteName` 来当整个的 `suiteName` 了。要是优化的话可能考虑采用新的数据结构，但是感觉很麻烦，还得再判断 `suiteName` 是不是变了啥的。。。先凑合用吧

- [x] 加入对修改 List.test.ets 的支持。感觉改起来也有点麻烦，而且要是按照之前的风格，import的时候还需要断行。真要改的话可能读取所有内容，在最前面加import，在最后面删 `)` 啥的，再加上当前测试套件。但也就复制一下，鼠标点一下的事儿，3秒整完了，不加这需求好像也行

- [ ] 用例描述的自动翻译。试了试很多描述因为太过sb，很难有好的翻译效果。但是好像也有讨巧的办法，毕竟这个描述没规定必须是用例名的英文翻译，我可以直接改成其他的好翻译的；或者就是每次手动翻译一下得了

  zmh写的用例，只翻译了是来自哪个组的：

  ```
      /*
       * @tc.number  SUB_ACE_UI_COMPONENT_LISTANDGRID_LIST_CAPABILITY_0040
       * @tc.name    SUB_ACE_UI_COMPONENT_LISTANDGRID_LIST_CAPABILITY_0040
       * @tc.desc    Component 3 groups
       * @tc.level   3
       */
  ```

  ​	

  甚至dw写的用例就没有这个用例描述，他是在name里面翻译了：

  ```
      /*
       * @tc.number : SUB_ACE_UI_OHOS_ANIMATOR_SIMPLE_CREATE_0040
       * @tc.name   : Test the missing optional parameters for the 'delay' method in the ohos.animator simple constructor.
       * @tc.type   : Function
       * @tc.size   : MediumTest
       * @tc.level  : 3
       */
  ```

  