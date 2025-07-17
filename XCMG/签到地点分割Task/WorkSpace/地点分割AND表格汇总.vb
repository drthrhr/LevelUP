Declare PtrSafe Function MessageBoxTimeout Lib "user32" Alias "MessageBoxTimeoutA" (ByVal hwnd As Long, ByVal lpText As String, ByVal lpCaption As String, ByVal wType As Long, ByVal wlange As Long, ByVal dwTimeout As Long) As Long



Sub 地点分割_查漏补缺()
    Dim folderPath As String
    Dim fileName As String
    Dim wb As Workbook
    Dim ws As Worksheet
    
    ' 获取当前工作簿所在目录
    folderPath = ThisWorkbook.Path & "\"
    
    ' 遍历文件夹内所有Excel文件（支持.xlsx和.xls）
    fileName = Dir(folderPath & "*.xls*")
    ' fileName = Dir(folderPath & "testData.xlsx")
    
    Application.ScreenUpdating = False  ' 关闭屏幕刷新加速
    Application.DisplayAlerts = False   ' 关闭警告提示
    
    Do While fileName <> ""
        If fileName <> ThisWorkbook.Name Then  ' 跳过自身
            Set wb = Workbooks.Open(folderPath & fileName)
            Set ws = wb.Sheets(1) ' 索引1代表第一个Sheet

            '------地点数据定位------
            Dim i As Long
            Dim j As Integer, addressCol As Integer
            Dim found As Boolean
            Dim cellValue As String
            ' 遍历第4行起的数据（跳过表头）
            For i = 4 To ws.UsedRange.Rows.Count
                For j = 1 To ws.UsedRange.Columns.Count
                    cellValue = CStr(ws.Cells(i, j).Value)
                    If InStr(cellValue, "省") > 0 And InStr(cellValue, "市") > 0 Then   ' 检测同时含"省"和"市"的地址
                        addressCol = j
                        found = True
                        Exit For
                    End If
                Next j
                If found Then Exit For
            Next i

            ' 若找到地址列则插入3列，并进行后续操作。
            If found Then
                Dim locationCol As Integer, provinceCol As Integer, cityCol As Integer, townCol As Integer
                locationCol = addressCol - 1    ' “地点”列序号
                addressCol = addressCol + 0    ' “详细地址”列序号
                provinceCol = addressCol + 1    ' “省”列序号
                cityCol = addressCol + 2    ' “市”列序号
                townCol = addressCol + 3    ' “街道”列序号

                ' 对“省”、“市”、“街道”列，设置其在第3行的表头信息
                ws.Cells(1, addressCol + 1).Resize(1, 3).EntireColumn.Insert
                ' ws.Cells(1, addressCol + 1).Resize(1, 4).EntireColumn.Insert
                ws.Cells(3, provinceCol).Value = "省"
                ws.Cells(3, cityCol).Value = "市"
                ws.Cells(3, townCol).Value = "街道"
                ' ws.Cells(4, townCol + 1).Value = "DEBUG"

                ' 设置要写入到“省”、“市”、“街道”列的公式模板
                formulaProvince = "=LEFT(A2,MIN(FIND({""省"",""市"",""区""},A2&""省市区"")))"
                formulaCity = "=LEFT(SUBSTITUTE(A2,LEFT(A2,MIN(FIND({""省"",""市"",""区""},A2&""省市区""))),""""),MIN(FIND({""市"",""区"",""县""},SUBSTITUTE(A2,LEFT(A2,MIN(FIND({""省"",""市"",""区""},A2&""省市区""))),"""")&""市区县"")))"
                formulaTown = "=LET(fullText,A2,streetPos,FIND(""街道"",fullText)+2,prefixText,LEFT(fullText,streetPos-1),cityPosArray,IFERROR(FIND(""市"",prefixText,SEQUENCE(LEN(prefixText))),0),lastCityPos,MAX(cityPosArray),adminPos,MAX(IFERROR(FIND({""省"",""市"",""区"",""县""},prefixText),0),lastCityPos),IF(adminPos>0,MID(prefixText,adminPos+1,streetPos-adminPos-3+2),LET(adminPos1,MAX(IFERROR(FIND({""省"",""区"",""县""},prefixText),0)),IF(adminPos1>0,MID(prefixText,adminPos1+1,streetPos-adminPos1-3+2),LEFT(prefixText,FIND(""街道"",prefixText)-1+2)))))"

                ' ------地点分割------
                Dim lastRow As Long
                Dim cityCell As String, addressCellValue As String, locationCellValue  As String
                Dim lastTownValue As String, lastaddressCellValue As String

                Dim pythonScript As String, shellCmd As String
                Dim regionParam As String, queryParam As String
                Dim result As String
                Dim shellObj As Object, exec As Object

                pythonScript = ThisWorkbook.Path & "\getTown.py"
                Set shellObj = CreateObject("WScript.Shell")
                lastRow = ws.UsedRange.Rows.Count
                ' lastRow = 30

                ' 从有数据的第4行开始，遍历所有行。
                ' 首先分割“省”、“市”列。
                ' 接着分割“街道”列。具体来说，就是看该行 addressCol 列的值，若包含“街道”，则写入公式分割地点；否则，调用python脚本获取街道信息并填入
                lastaddressCellValue = ""   ' 上一次的“详细地址”值在最开始初始化为空字符串
                For i = 4 To lastRow
                    ' 分别获取该行“地点”、“详细地址”列的值
                    locationCellValue = ws.Cells(i, locationCol).Value
                    addressCellValue = ws.Cells(i, addressCol).Value

                    ' 分割“省”、“市”列。写入公式分割地点
                    ws.Cells(i, provinceCol).Formula = Replace(formulaProvince, "A2", ws.Cells(i, addressCol).Address(0, 0))    ' 写入分割“省”的公式，并将其中的“A2”替换为当前单元格的相对地址，比如“H4”等
                    ws.Cells(i, cityCol).Formula = Replace(formulaCity, "A2", ws.Cells(i, addressCol).Address(0, 0))    ' 写入分割“市”的公式，并将其中的“A2”替换为当前单元格的相对地址，比如“H4”等

                    ' 分割“街道”列。先判断本次的“详细地址”是否与上一次的“详细地址”相同。若相同则可以直接使用上一次的街道值，尽量减少插入公式或调用python脚本的次数以加快速度；若不同，则依据情况选择插入公式或调用python脚本。
                    ' 查漏补缺。只有“街道”列为空时才进行
                    If IsEmpty(ws.Cells(i, townCol)) Then
                        If StrComp(addressCellValue, lastaddressCellValue, vbTextCompare) = 0 Then ' 直接使用上一次的街道值
                            ws.Cells(i, townCol).Value = ws.Cells(i - 1, townCol).Value

                            ' ws.Cells(i, townCol + 1).Value = "Cached"

                        Else    ' 若不同，则看当前的“详细地址”列的值是否包含“街道”
                            If InStr(addressCellValue, "街道") > 0 Then ' 若包含“街道”，则写入公式分割地点
                                ws.Cells(i, townCol).Formula = Replace(formulaTown, "A2", ws.Cells(i, addressCol).Address(0, 0))    ' 写入分割“街道”的公式，并将其中的“A2”替换为当前单元格的相对地址，比如“H4”等

                                ' ws.Cells(i, townCol + 1).Value = "Formulated"

                            Else    ' 若不包含“街道”，则调用python脚本获取街道信息并填入
                                regionParam = ws.Cells(i, cityCol).Value    ' 查询区域设置为“市”那一列的值
                                queryParam = addressCellValue & locationCellValue     ' 查询关键词设置为“详细地址”列的值拼接上“地点”列的值
                                ' 拼接命令并执行python脚本，把python脚本输出的结果写入“街道”列中
                                shellCmd = "pythonw " & pythonScript & " " & regionParam & " " & queryParam

                                ' ws.Cells(i, townCol + 1).Value = shellCmd

                                Set exec = shellObj.exec(shellCmd)
                                result = exec.StdOut.ReadAll

                                If Trim(result) = "" Then   ' 若结果为空，则尝试查询关键词只设置为“详细地址”列的值
                                    queryParam = addressCellValue
                                    shellCmd = "pythonw " & pythonScript & " " & regionParam & " " & queryParam
                                    Set exec = shellObj.exec(shellCmd)
                                    result = exec.StdOut.ReadAll
                                End If
                                If Trim(result) = "" Then   ' 若结果仍为空，则尝试查询关键词只设置为“地点”列的值
                                    queryParam = locationCellValue
                                    shellCmd = "pythonw " & pythonScript & " " & regionParam & " " & queryParam
                                    Set exec = shellObj.exec(shellCmd)
                                    result = exec.StdOut.ReadAll
                                End If

                                ws.Cells(i, townCol).Value = result
                                Set exec = Nothing
                            End If
                        End If
                        lastaddressCellValue = ws.Cells(i, addressCol).Value   ' 更新上一次的“详细地址”值为当前的“详细地址”值
                        wb.Save
                    End If
                Next i

            Else
                MsgBox "文件 " & fileName & " 未检测到地址列"
            End If
            
            wb.Close SaveChanges:=True  ' 保存并关闭
        End If
        fileName = Dir()  ' 获取下一个文件
    Loop
    
    ' 恢复Excel设置
    Application.ScreenUpdating = True
    Application.DisplayAlerts = True

    MsgBox "地点分割操作完成！"
    ' MessageBoxTimeout 0, "地点分割操作完成！(2s后自动关闭)", "地点分割", 0, 1, 2000
End Sub


Sub 地点分割_手动()
    Dim folderPath As String
    Dim fileName As String
    Dim wb As Workbook
    Dim ws As Worksheet
    
    ' 获取当前工作簿所在目录
    folderPath = ThisWorkbook.Path & "\"
    
    ' 遍历文件夹内所有Excel文件（支持.xlsx和.xls）
    fileName = Dir(folderPath & "*.xls*")
    ' fileName = Dir(folderPath & "testData.xlsx")
    
    Application.ScreenUpdating = False  ' 关闭屏幕刷新加速
    Application.DisplayAlerts = False   ' 关闭警告提示
    
    Do While fileName <> ""
        If fileName <> ThisWorkbook.Name Then  ' 跳过自身
            Set wb = Workbooks.Open(folderPath & fileName)
            Set ws = wb.Sheets(1) ' 索引1代表第一个Sheet

            '------地点数据定位------
            Dim i As Long
            Dim j As Integer, addressCol As Integer
            Dim found As Boolean
            Dim cellValue As String
            ' 遍历第4行起的数据（跳过表头）
            For i = 4 To ws.UsedRange.Rows.Count
                For j = 1 To ws.UsedRange.Columns.Count
                    cellValue = CStr(ws.Cells(i, j).Value)
                    If InStr(cellValue, "省") > 0 And InStr(cellValue, "市") > 0 Then   ' 检测同时含"省"和"市"的地址
                        addressCol = j
                        found = True
                        Exit For
                    End If
                Next j
                If found Then Exit For
            Next i

            ' 若找到地址列则插入3列，并进行后续操作。
            If found Then
                Dim locationCol As Integer, provinceCol As Integer, cityCol As Integer, townCol As Integer
                locationCol = addressCol - 1    ' “地点”列序号
                addressCol = addressCol + 0    ' “详细地址”列序号
                provinceCol = addressCol + 1    ' “省”列序号
                cityCol = addressCol + 2    ' “市”列序号
                townCol = addressCol + 3    ' “街道”列序号

                ' 对“省”、“市”、“街道”列，设置其在第3行的表头信息
                ws.Cells(1, addressCol + 1).Resize(1, 3).EntireColumn.Insert
                ' ws.Cells(1, addressCol + 1).Resize(1, 4).EntireColumn.Insert
                ws.Cells(3, provinceCol).Value = "省"
                ws.Cells(3, cityCol).Value = "市"
                ws.Cells(3, townCol).Value = "街道"
                ' ws.Cells(4, townCol + 1).Value = "DEBUG"

                ' 设置要写入到“省”、“市”、“街道”列的公式模板
                formulaProvince = "=LEFT(A2,MIN(FIND({""省"",""市"",""区""},A2&""省市区"")))"
                formulaCity = "=LEFT(SUBSTITUTE(A2,LEFT(A2,MIN(FIND({""省"",""市"",""区""},A2&""省市区""))),""""),MIN(FIND({""市"",""区"",""县""},SUBSTITUTE(A2,LEFT(A2,MIN(FIND({""省"",""市"",""区""},A2&""省市区""))),"""")&""市区县"")))"
                formulaTown = "=LET(fullText,A2,streetPos,FIND(""街道"",fullText)+2,prefixText,LEFT(fullText,streetPos-1),cityPosArray,IFERROR(FIND(""市"",prefixText,SEQUENCE(LEN(prefixText))),0),lastCityPos,MAX(cityPosArray),adminPos,MAX(IFERROR(FIND({""省"",""市"",""区"",""县""},prefixText),0),lastCityPos),IF(adminPos>0,MID(prefixText,adminPos+1,streetPos-adminPos-3+2),LET(adminPos1,MAX(IFERROR(FIND({""省"",""区"",""县""},prefixText),0)),IF(adminPos1>0,MID(prefixText,adminPos1+1,streetPos-adminPos1-3+2),LEFT(prefixText,FIND(""街道"",prefixText)-1+2)))))"

                ' ------地点分割------
                Dim lastRow As Long
                Dim cityCell As String, addressCellValue As String, locationCellValue  As String
                Dim lastTownValue As String, lastaddressCellValue As String

                Dim pythonScript As String, shellCmd As String
                Dim regionParam As String, queryParam As String
                Dim result As String
                Dim shellObj As Object, exec As Object

                pythonScript = ThisWorkbook.Path & "\getTown.py"
                Set shellObj = CreateObject("WScript.Shell")
                lastRow = ws.UsedRange.Rows.Count
                ' lastRow = 30

                ' 从有数据的第4行开始，遍历所有行。
                ' 首先分割“省”、“市”列。
                ' 接着分割“街道”列。具体来说，就是看该行 addressCol 列的值，若包含“街道”，则写入公式分割地点；否则，调用python脚本获取街道信息并填入
                lastaddressCellValue = ""   ' 上一次的“详细地址”值在最开始初始化为空字符串
                For i = 4 To lastRow
                    ' 分别获取该行“地点”、“详细地址”列的值
                    locationCellValue = ws.Cells(i, locationCol).Value
                    addressCellValue = ws.Cells(i, addressCol).Value

                    ' 分割“省”、“市”列。写入公式分割地点
                    ws.Cells(i, provinceCol).Formula = Replace(formulaProvince, "A2", ws.Cells(i, addressCol).Address(0, 0))    ' 写入分割“省”的公式，并将其中的“A2”替换为当前单元格的相对地址，比如“H4”等
                    ws.Cells(i, cityCol).Formula = Replace(formulaCity, "A2", ws.Cells(i, addressCol).Address(0, 0))    ' 写入分割“市”的公式，并将其中的“A2”替换为当前单元格的相对地址，比如“H4”等

                    ' 分割“街道”列。先判断本次的“详细地址”是否与上一次的“详细地址”相同。若相同则可以直接使用上一次的街道值，尽量减少插入公式或调用python脚本的次数以加快速度；若不同，则依据情况选择插入公式或调用python脚本。
                    If StrComp(addressCellValue, lastaddressCellValue, vbTextCompare) = 0 Then ' 直接使用上一次的街道值
                        ws.Cells(i, townCol).Value = ws.Cells(i - 1, townCol).Value

                        ' ws.Cells(i, townCol + 1).Value = "Cached"

                    Else    ' 若不同，则看当前的“详细地址”列的值是否包含“街道”
                        If InStr(addressCellValue, "街道") > 0 Then ' 若包含“街道”，则写入公式分割地点
                            ws.Cells(i, townCol).Formula = Replace(formulaTown, "A2", ws.Cells(i, addressCol).Address(0, 0))    ' 写入分割“街道”的公式，并将其中的“A2”替换为当前单元格的相对地址，比如“H4”等

                            ' ws.Cells(i, townCol + 1).Value = "Formulated"

                        Else    ' 若不包含“街道”，则调用python脚本获取街道信息并填入
                            regionParam = ws.Cells(i, cityCol).Value    ' 查询区域设置为“市”那一列的值
                            queryParam = addressCellValue & locationCellValue     ' 查询关键词设置为“详细地址”列的值拼接上“地点”列的值
                            ' 拼接命令并执行python脚本，把python脚本输出的结果写入“街道”列中
                            shellCmd = "pythonw " & pythonScript & " " & regionParam & " " & queryParam

                            ' ws.Cells(i, townCol + 1).Value = shellCmd

                            Set exec = shellObj.exec(shellCmd)
                            result = exec.StdOut.ReadAll

                            If Trim(result) = "" Then   ' 若结果为空，则尝试查询关键词只设置为“详细地址”列的值
                                queryParam = addressCellValue
                                shellCmd = "pythonw " & pythonScript & " " & regionParam & " " & queryParam
                                Set exec = shellObj.exec(shellCmd)
                                result = exec.StdOut.ReadAll
                            End If
                            If Trim(result) = "" Then   ' 若结果仍为空，则尝试查询关键词只设置为“地点”列的值
                                queryParam = locationCellValue
                                shellCmd = "pythonw " & pythonScript & " " & regionParam & " " & queryParam
                                Set exec = shellObj.exec(shellCmd)
                                result = exec.StdOut.ReadAll
                            End If

                            ws.Cells(i, townCol).Value = result
                            Set exec = Nothing
                        End If
                    End If
                    lastaddressCellValue = ws.Cells(i, addressCol).Value   ' 更新上一次的“详细地址”值为当前的“详细地址”值
                    wb.Save
                Next i

            Else
                MsgBox "文件 " & fileName & " 未检测到地址列"
            End If
            
            wb.Close SaveChanges:=True  ' 保存并关闭
        End If
        fileName = Dir()  ' 获取下一个文件
    Loop
    
    ' 恢复Excel设置
    Application.ScreenUpdating = True
    Application.DisplayAlerts = True

    MsgBox "地点分割操作完成！"
    ' MessageBoxTimeout 0, "地点分割操作完成！(2s后自动关闭)", "地点分割", 0, 1, 2000
End Sub




Sub 表格汇总_手动()
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
    destRow = 1 ' 数据从第 2 行开始（保留表头位置）
    
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
    
    ' 恢复设置
    Application.ScreenUpdating = True
    Application.Calculation = xlCalculationAutomatic

    ' MessageBoxTimeout 0, "表格汇总操作完成！(2s后自动关闭)", "表格汇总", 0, 1, 2000
    MsgBox "表格汇总操作完成！"
End Sub



Sub 地点分割WithAutoCloseMsg(Optional ByVal autoCloseMsg As Boolean)
    Dim folderPath As String
    Dim fileName As String
    Dim wb As Workbook
    Dim ws As Worksheet
    
    ' 获取当前工作簿所在目录
    folderPath = ThisWorkbook.Path & "\"
    
    ' 遍历文件夹内所有Excel文件（支持.xlsx和.xls）
    fileName = Dir(folderPath & "*.xls*")
    ' fileName = Dir(folderPath & "testData.xlsx")
    
    Application.ScreenUpdating = False  ' 关闭屏幕刷新加速
    Application.DisplayAlerts = False   ' 关闭警告提示
    
    Do While fileName <> ""
        If fileName <> ThisWorkbook.Name Then  ' 跳过自身
            Set wb = Workbooks.Open(folderPath & fileName)
            Set ws = wb.Sheets(1) ' 索引1代表第一个Sheet

            '------地点数据定位------
            Dim i As Long
            Dim j As Integer, addressCol As Integer
            Dim found As Boolean
            Dim cellValue As String
            ' 遍历第4行起的数据（跳过表头）
            For i = 4 To ws.UsedRange.Rows.Count
                For j = 1 To ws.UsedRange.Columns.Count
                    cellValue = CStr(ws.Cells(i, j).Value)
                    If InStr(cellValue, "省") > 0 And InStr(cellValue, "市") > 0 Then   ' 检测同时含"省"和"市"的地址
                        addressCol = j
                        found = True
                        Exit For
                    End If
                Next j
                If found Then Exit For
            Next i

            ' 若找到地址列则插入3列，并进行后续操作。
            If found Then
                Dim locationCol As Integer, provinceCol As Integer, cityCol As Integer, townCol As Integer
                locationCol = addressCol - 1    ' “地点”列序号
                addressCol = addressCol + 0    ' “详细地址”列序号
                provinceCol = addressCol + 1    ' “省”列序号
                cityCol = addressCol + 2    ' “市”列序号
                townCol = addressCol + 3    ' “街道”列序号

                ' 对“省”、“市”、“街道”列，设置其在第3行的表头信息
                ws.Cells(1, addressCol + 1).Resize(1, 3).EntireColumn.Insert
                ' ws.Cells(1, addressCol + 1).Resize(1, 4).EntireColumn.Insert
                ws.Cells(3, provinceCol).Value = "省"
                ws.Cells(3, cityCol).Value = "市"
                ws.Cells(3, townCol).Value = "街道"
                ' ws.Cells(4, townCol + 1).Value = "DEBUG"

                ' 设置要写入到“省”、“市”、“街道”列的公式模板
                formulaProvince = "=LEFT(A2,MIN(FIND({""省"",""市"",""区""},A2&""省市区"")))"
                formulaCity = "=LEFT(SUBSTITUTE(A2,LEFT(A2,MIN(FIND({""省"",""市"",""区""},A2&""省市区""))),""""),MIN(FIND({""市"",""区"",""县""},SUBSTITUTE(A2,LEFT(A2,MIN(FIND({""省"",""市"",""区""},A2&""省市区""))),"""")&""市区县"")))"
                formulaTown = "=LET(fullText,A2,streetPos,FIND(""街道"",fullText)+2,prefixText,LEFT(fullText,streetPos-1),cityPosArray,IFERROR(FIND(""市"",prefixText,SEQUENCE(LEN(prefixText))),0),lastCityPos,MAX(cityPosArray),adminPos,MAX(IFERROR(FIND({""省"",""市"",""区"",""县""},prefixText),0),lastCityPos),IF(adminPos>0,MID(prefixText,adminPos+1,streetPos-adminPos-3+2),LET(adminPos1,MAX(IFERROR(FIND({""省"",""区"",""县""},prefixText),0)),IF(adminPos1>0,MID(prefixText,adminPos1+1,streetPos-adminPos1-3+2),LEFT(prefixText,FIND(""街道"",prefixText)-1+2)))))"

                ' ------地点分割------
                Dim lastRow As Long
                Dim cityCell As String, addressCellValue As String, locationCellValue  As String
                Dim lastTownValue As String, lastaddressCellValue As String

                Dim pythonScript As String, shellCmd As String
                Dim regionParam As String, queryParam As String
                Dim result As String
                Dim shellObj As Object, exec As Object

                pythonScript = ThisWorkbook.Path & "\getTown.py"
                Set shellObj = CreateObject("WScript.Shell")
                lastRow = ws.UsedRange.Rows.Count
                ' lastRow = 30

                ' 从有数据的第4行开始，遍历所有行。
                ' 首先分割“省”、“市”列。
                ' 接着分割“街道”列。具体来说，就是看该行 addressCol 列的值，若包含“街道”，则写入公式分割地点；否则，调用python脚本获取街道信息并填入
                lastaddressCellValue = ""   ' 上一次的“详细地址”值在最开始初始化为空字符串
                For i = 4 To lastRow
                    ' 分别获取该行“地点”、“详细地址”列的值
                    locationCellValue = ws.Cells(i, locationCol).Value
                    addressCellValue = ws.Cells(i, addressCol).Value

                    ' 分割“省”、“市”列。写入公式分割地点
                    ws.Cells(i, provinceCol).Formula = Replace(formulaProvince, "A2", ws.Cells(i, addressCol).Address(0, 0))    ' 写入分割“省”的公式，并将其中的“A2”替换为当前单元格的相对地址，比如“H4”等
                    ws.Cells(i, cityCol).Formula = Replace(formulaCity, "A2", ws.Cells(i, addressCol).Address(0, 0))    ' 写入分割“市”的公式，并将其中的“A2”替换为当前单元格的相对地址，比如“H4”等

                    ' 分割“街道”列。先判断本次的“详细地址”是否与上一次的“详细地址”相同。若相同则可以直接使用上一次的街道值，尽量减少插入公式或调用python脚本的次数以加快速度；若不同，则依据情况选择插入公式或调用python脚本。
                    If StrComp(addressCellValue, lastaddressCellValue, vbTextCompare) = 0 Then ' 直接使用上一次的街道值
                        ws.Cells(i, townCol).Value = ws.Cells(i - 1, townCol).Value

                        ' ws.Cells(i, townCol + 1).Value = "Cached"

                    Else    ' 若不同，则看当前的“详细地址”列的值是否包含“街道”
                        If InStr(addressCellValue, "街道") > 0 Then ' 若包含“街道”，则写入公式分割地点
                            ws.Cells(i, townCol).Formula = Replace(formulaTown, "A2", ws.Cells(i, addressCol).Address(0, 0))    ' 写入分割“街道”的公式，并将其中的“A2”替换为当前单元格的相对地址，比如“H4”等

                            ' ws.Cells(i, townCol + 1).Value = "Formulated"

                        Else    ' 若不包含“街道”，则调用python脚本获取街道信息并填入
                            regionParam = ws.Cells(i, cityCol).Value    ' 查询区域设置为“市”那一列的值
                            queryParam = addressCellValue & locationCellValue     ' 查询关键词设置为“详细地址”列的值拼接上“地点”列的值
                            ' 拼接命令并执行python脚本，把python脚本输出的结果写入“街道”列中
                            shellCmd = "pythonw " & pythonScript & " " & regionParam & " " & queryParam

                            ' ws.Cells(i, townCol + 1).Value = shellCmd

                            Set exec = shellObj.exec(shellCmd)
                            result = exec.StdOut.ReadAll

                            If Trim(result) = "" Then   ' 若结果为空，则尝试查询关键词只设置为“详细地址”列的值
                                queryParam = addressCellValue
                                shellCmd = "pythonw " & pythonScript & " " & regionParam & " " & queryParam
                                Set exec = shellObj.exec(shellCmd)
                                result = exec.StdOut.ReadAll
                            End If
                            If Trim(result) = "" Then   ' 若结果仍为空，则尝试查询关键词只设置为“地点”列的值
                                queryParam = locationCellValue
                                shellCmd = "pythonw " & pythonScript & " " & regionParam & " " & queryParam
                                Set exec = shellObj.exec(shellCmd)
                                result = exec.StdOut.ReadAll
                            End If

                            ws.Cells(i, townCol).Value = result
                            Set exec = Nothing
                        End If
                    End If
                    lastaddressCellValue = ws.Cells(i, addressCol).Value   ' 更新上一次的“详细地址”值为当前的“详细地址”值
                    wb.Save
                Next i

            Else
                MsgBox "文件 " & fileName & " 未检测到地址列"
            End If
            
            wb.Close SaveChanges:=True  ' 保存并关闭
        End If
        fileName = Dir()  ' 获取下一个文件
    Loop
    
    ' 恢复Excel设置
    Application.ScreenUpdating = True
    Application.DisplayAlerts = True

    If IsMissing(autoCloseMsg) Then
        MsgBox "地点分割操作完成！"
    Else
        If autoCloseMsg = True Then
            MessageBoxTimeout 0, "地点分割操作完成！(2s后自动关闭)", "地点分割", 0, 1, 2000
        Else
            MsgBox "地点分割操作完成！"
        End If
    End If

    ' MsgBox "地点分割操作完成！", vbInformation
    ' MessageBoxTimeout 0, "地点分割操作完成！(2s后自动关闭)", "地点分割", 0, 1, 2000
End Sub




Sub 表格汇总WithAutoCloseMsg(Optional ByVal autoCloseMsg As Boolean)
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
    destRow = 1 ' 数据从第 2 行开始（保留表头位置）
    
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
    
    ' 恢复设置
    Application.ScreenUpdating = True
    Application.Calculation = xlCalculationAutomatic

    If IsMissing(autoCloseMsg) Then
        MsgBox "表格汇总操作完成！"
    Else
        If autoCloseMsg = True Then
            MessageBoxTimeout 0, "表格汇总操作完成！(2s后自动关闭)", "表格汇总", 0, 1, 2000
        Else
            MsgBox "表格汇总操作完成！"
        End If
    End If

    ' MessageBoxTimeout 0, "表格汇总完成！(2s后自动关闭)", "表格汇总", 0, 1, 2000
    ' MsgBox "合并完成！"
End Sub


Sub 地点分割AND表格汇总()

    地点分割WithAutoCloseMsg(True)
    表格汇总WithAutoCloseMsg(True)
    MsgBox "地点分割、表格汇总操作均完成！"

End Sub