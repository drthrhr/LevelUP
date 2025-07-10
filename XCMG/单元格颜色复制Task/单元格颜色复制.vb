Sub copy_paste_with_format()
    Dim i As Long
    Dim var As Variant

    Dim toChange As Worksheet
    Dim reference As Worksheet

    Set toChange = Application.Worksheets("1月明细表")  '“要改的表”的Sheet名
    Set reference = Application.Worksheets("12月明细表")  '要“参考的表”的Sheet名

    For i = 3 To toChange.UsedRange.Rows.Count  '“要改的表”中，从第几行开始改颜色，就写 i=几
     
     'toChange.Range("D" & i)中，D为 “要改的表” 里合同号那一列的列号，可视情况修改
     'reference.Range("D:D")中，D:D为 “参考的表” 里合同号那一列的列号，可视情况修改
     var = Application.Match(toChange.Range("D" & i), reference.Range("D:D"), 0)

     If Not IsError(var) Then

      '→这行代码含义是，当找到匹配的合同号之后，“要改的表”里A列到I列的颜色，全给改成“参考的表”里D列的颜色。
      '左边的toChange.Range("A" & i & ":" & "I" & i)：括号里的内容就是指定要更改颜色的范围，可视情况修改。
      '     *i其实就代表当前的行号，&是连接符，括号里的意思，其实就是excel里的范围 Ai：Ii （i代表的是一个数字）
      '     *改的时候注意两点：1.范围中除了行号是数字，其他的都算字符，要用英文双引号括起来，比如列号字母、引号；2.要记得在字母之间加上连接符&
      '     *比如，当前更改颜色的范围是A:I，想改成A:C还有I:K两个范围，比如当i为5时，对应excel里的写法为【A5:C5,I5:K5】，那就可以写成 toChange.Range("A" & i & ":" & "C" & i & "," & "I" & i & ":" & "K" & i)
      '右边的reference.Range("D" & var)：这里指定要改成“参考的表”里哪一列的颜色，当前是改成D列的颜色。
      toChange.Range("A" & i & ":" & "J" & i).Interior.Color = reference.Range("D" & var).Interior.Color

      '→这行代码含义是，在上一步把整个A:I范围的颜色都改掉的基础之上，再改某个需要额外变更颜色的列。这里是把“要改的表”的A列给额外改变颜色，改成“参考的表”里A列的颜色。
      '(我把代码给注释了，所以它不会起作用。若想让它起作用，把代码前面的英文引号删去就行)
      '左边的toChange.Range("A" & i)：指定“要改的表”里，要改哪一列的颜色，当前是A列
      '右边的reference.Range("A" & var)：指定“参考的表”里，参考哪一列的颜色去改，当前是A列
      '比如，如果要再额外把“要改的表”的B列改成“参考的表”里E列的颜色，那么需要多加一行代码： toChange.Range("B" & i).Interior.Color = reference.Range("E" & var).Interior.Color
      'toChange.Range("A" & i).Interior.Color = reference.Range("A" & var).Interior.Color

     End If
    Next i
End Sub