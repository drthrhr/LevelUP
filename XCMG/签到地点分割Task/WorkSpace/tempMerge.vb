Sub MergeAllExcelFilesWithFormat()
    Application.ScreenUpdating = False
    Application.Calculation = xlCalculationManual
    
    Dim path As String, file As String
    Dim wbSource As Workbook, wsSource As Worksheet
    Dim wbResult As Workbook, wsResult As Worksheet
    Dim headers As Object ' 存储所有列标题的字典
    Dim colDict As Object
    Dim lastRow As Long, lastCol As Long
    Dim destRow As Long, destCol As Long
    Dim colName As String, colWidth As Double
    
    ' 初始化字典（用于列名去重和索引映射）
    Set headers = CreateObject("Scripting.Dictionary")  ' 存储第一阶段收集到的所有列的列名
    Set colDict = CreateObject("Scripting.Dictionary")
    
    ' 设置当前目录路径
    path = ThisWorkbook.path & "\"
    file = Dir(path & "*.xls*")
    
    ' 设置汇总后的最终表格为当前工作簿的第一个Sheet
    Set wsResult = ThisWorkbook.Sheets(1)
    destRow = 1
    
    ' --- 第一阶段：收集所有列标题及列宽 ---
    Do While file <> ""
        If file <> ThisWorkbook.Name Then ' 跳过自身
            Set wbSource = Workbooks.Open(path & file)
            Set wsSource = wbSource.Sheets(1)
            
            ' 读取第 3 行作为列标题。遍历该文件所有列，记录列名及顺序，到字典 headers 中，同时保持字典 headers 中没有重复的值
            lastCol = wsSource.Cells(3, wsSource.Columns.Count).End(xlToLeft).Column    ' 获取总列数
            For col = 1 To lastCol
                colName = Trim(wsSource.Cells(3, col).Value)
                If colName <> "" Then
                    ' 记录列名、最大列宽及源格式样例
                    If Not headers.Exists(colName) Then
                        headers.Add colName, Array(headers.Count + 1, 0, wsSource.Cells(3, col).Font.Bold) ' [索引, 列宽, 是否加粗]
                    End If
                    ' 更新最大列宽
                    colWidth = wsSource.Columns(col).ColumnWidth
                    If colWidth > headers(colName)(1) Then headers(colName)(1) = colWidth
                End If
            Next col
            wbSource.Close False ' 关闭源文件不保存
        End If
        file = Dir()
    Loop
    
    ' --- 第二阶段：生成完整表头并应用格式 ---
    For Each key In headers.keys
        destCol = headers(key)(0)
        wsResult.Cells(3, destCol).Value = key
        ' 应用列宽和加粗格式
        wsResult.Columns(destCol).ColumnWidth = headers(key)(1)
        wsResult.Cells(3, destCol).Font.Bold = headers(key)(2)
    Next key
    destRow = 4 ' 数据从第4行开始
    
    ' --- 第三阶段：合并数据并复制格式 ---
    file = Dir(path & "*.xls*")
    Do While file <> ""
        If file <> ThisWorkbook.Name Then
            Set wbSource = Workbooks.Open(path & file, ReadOnly:=True)
            Set wsSource = wbSource.Sheets(1)
            
            ' 建立当前文件的列名映射字典。遍历该文件所有列，记录列名及顺序，到字典 colDict 中，同时保持字典 colDict 中没有重复的值
            colDict.RemoveAll
            lastCol = wsSource.Cells(3, Columns.Count).End(xlToLeft).Column   ' 获取总列数
            For col = 1 To lastCol
                colName = Trim(wsSource.Cells(3, col).Value)
                If colName <> "" Then colDict.Add colName, col
            Next col
            
            ' 复制数据（跳过1-3行表头）
            lastRow = wsSource.Cells(Rows.Count, 1).End(xlUp).Row
            For srcRow = 4 To lastRow
                destRow = destRow + 1
                ' 遍历所有列标题，匹配则复制，否则留空
                For Each hKey In headers.keys
                    destCol = headers(hKey)(0)
                    If colDict.Exists(hKey) Then
                        ' 复制值和格式
                        wsSource.Cells(srcRow, colDict(hKey)).Copy
                        wsResult.Cells(destRow, destCol).PasteSpecial Paste:=xlPasteAll
                    Else
                        wsResult.Cells(destRow, destCol).Value = "" ' 缺失列置空
                    End If
                Next hKey
            Next srcRow
            
            ' 复制第3行表头格式（字体/背景色/边框等）
            wsSource.Rows(3).Copy
            wsResult.Rows(3).PasteSpecial Paste:=xlPasteFormats
            Application.CutCopyMode = False ' 清除剪贴板
            
            wbSource.Close False
        End If
        file = Dir()
    Loop
    
    ' 格式化和清理
    ' wsResult.Rows("1:2").Insert
    ' wsResult.Cells(1, 1).Value = "合并时间：" & Now()
    ' wsResult.UsedRange.Columns.AutoFit
    
    Application.ScreenUpdating = True
    Application.Calculation = xlCalculationAutomatic
    MsgBox "合并完成！"
End Sub