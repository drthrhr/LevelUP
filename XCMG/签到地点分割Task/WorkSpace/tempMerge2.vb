Sub MergeAllExcelFiles()
    ' 关闭屏幕更新和自动计算以提升性能
    Application.ScreenUpdating = False
    Application.Calculation = xlCalculationManual
    
    ' 定义变量
    Dim path As String
    Dim file As String
    Dim wbSource As Workbook
    Dim wsSource As Worksheet
    Dim wbResult As Workbook
    Dim wsResult As Worksheet
    Dim headers As Object ' 存储所有列标题的字典
    Dim colDict As Object
    Dim lastRow As Long, lastCol As Long
    Dim destRow As Long, destCol As Long
    Dim colName As String
    
    ' 初始化字典（用于列名去重和索引映射）
    Set headers = CreateObject("Scripting.Dictionary")  ' 存储第一阶段收集到的所有列的列名
    Set colDict = CreateObject("Scripting.Dictionary")
    
    ' 设置当前目录路径
    path = ThisWorkbook.path & "\"
    file = Dir(path & "*.xls*")
    
    ' 创建结果工作簿和工作表
    ' Set wbResult = Workbooks.Add
    Set wsResult = ThisWorkbook.Sheets(1)
    ' wsResult.Name = "合并结果"
    destRow = 1 ' 结果表起始行
    
    ' --- 第一阶段：收集所有列标题 ---
    Do While file <> ""
        If file <> ThisWorkbook.Name Then ' 跳过自身
            Set wbSource = Workbooks.Open(path & file)
            Set wsSource = wbSource.Sheets(1)
            
            ' 读取第 3 行作为列标题。遍历该文件所有列，记录列名及顺序，到字典 headers 中，同时保持字典 headers 中没有重复的值
            lastCol = wsSource.Cells(3, wsSource.Columns.Count).End(xlToLeft).Column    ' 获取总列数
            For col = 1 To lastCol
                colName = Trim(wsSource.Cells(3, col).Value)
                If colName <> "" And Not headers.Exists(colName) Then
                    headers.Add colName, headers.Count + 1 ' 记录列名及顺序
                End If
            Next col
            
            wbSource.Close False ' 关闭源文件不保存
        End If
        file = Dir()
    Loop
    
    ' --- 第二阶段：在结果表中生成完整表头 ---
    For Each key In headers.keys
        wsResult.Cells(1, headers(key)).Value = key
    Next key
    destRow = 4 ' 数据从第 4 行开始（保留表头位置）
    
    ' --- 第三阶段：合并数据 ---
    file = Dir(path & "*.xls*")
    Do While file <> ""
        If file <> ThisWorkbook.Name Then
            Set wbSource = Workbooks.Open(path & file)
            Set wsSource = wbSource.Sheets(1)
            
            ' 建立当前文件的列名映射字典。遍历该文件所有列，记录列名及顺序，到字典 colDict 中，同时保持字典 colDict 中没有重复的值
            colDict.RemoveAll
            lastCol = wsSource.Cells(3, wsSource.Columns.Count).End(xlToLeft).Column    ' 获取总列数
            For col = 1 To lastCol
                colName = Trim(wsSource.Cells(3, col).Value)
                If colName <> "" Then colDict.Add colName, col
            Next col
            
            ' 复制数据（跳过1-3行表头）
            lastRow = wsSource.Cells(wsSource.Rows.Count, 1).End(xlUp).Row
            For srcRow = 4 To lastRow
                destRow = destRow + 1
                ' 遍历所有列标题，匹配则复制，否则留空
                For Each hKey In headers.keys
                    destCol = headers(hKey)
                    If colDict.Exists(hKey) Then
                        wsResult.Cells(destRow, destCol).Value = wsSource.Cells(srcRow, colDict(hKey)).Value
                    Else
                        wsResult.Cells(destRow, destCol).Value = "" ' 缺失列置空
                    End If
                Next hKey
            Next srcRow
            
            wbSource.Close False
        End If
        file = Dir()
    Loop
    
    ' 格式化和清理
    ' wsResult.Rows("1:3").Insert ' 预留原始表头位置
    ' wsResult.Cells(3, 1).Value = "合并时间：" & Now()
    ' wsResult.Cells.Columns.AutoFit
    
    ' 恢复设置
    Application.ScreenUpdating = True
    Application.Calculation = xlCalculationAutomatic
    MsgBox "合并完成！"
End Sub