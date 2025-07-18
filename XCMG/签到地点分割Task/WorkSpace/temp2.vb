Sub Test()
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
            ' 查漏补缺时不再插入
            If found Then
                Dim locationCol As Integer, provinceCol As Integer, cityCol As Integer, townCol As Integer, longitudeCol As Integer, latitudeCol As Integer
                longitudeCol = addressCol - 3   ' “经度”列序号
                latitudeCol = addressCol - 2    ' “纬度”列序号
                locationCol = addressCol - 1    ' “地点”列序号
                addressCol = addressCol + 0    ' “详细地址”列序号
                provinceCol = addressCol + 1    ' “省”列序号
                cityCol = addressCol + 2    ' “市”列序号
                townCol = addressCol + 3    ' “街道”列序号

                ' 对“省”、“市”、“街道”列，设置其在第3行的表头信息
                ' ws.Cells(1, addressCol + 1).Resize(1, 3).EntireColumn.Insert
                ' ws.Cells(3, provinceCol).Value = "省"
                ' ws.Cells(3, cityCol).Value = "市"
                ' ws.Cells(3, townCol).Value = "街道"

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
                Dim longitudeParam As String, latitudeParam As String
                Dim result As String
                Dim shellObj As Object, exec As Object
            
                Set shellObj = CreateObject("WScript.Shell")
                lastRow = ws.UsedRange.Rows.Count
                ' lastRow = 30

                ' 从有数据的第4行开始，遍历所有行。
                ' 首先分割“省”、“市”列。
                ' 接着分割“街道”列。具体来说，就是看该行 addressCol 列的值，若包含“街道”，则写入公式分割地点；否则，调用python脚本获取街道信息并填入
                lastaddressCellValue = ""   ' 上一次的“详细地址”值在最开始初始化为空字符串
                lastTownValue = ""
                For i = 4 To lastRow
                    ' 分别获取该行“地点”、“详细地址”列的值
                    locationCellValue = ws.Cells(i, locationCol).Value
                    addressCellValue = ws.Cells(i, addressCol).Value

                    ' 分割“省”、“市”列。写入公式分割地点
                    ' 因为是查漏补缺，所以如果是空的再写入，不是空的就不写入了，省得万一之前手动改了啥再覆盖掉
                    If IsEmpty(ws.Cells(i, provinceCol)) Then
                        ws.Cells(i, provinceCol).Formula = Replace(formulaProvince, "A2", ws.Cells(i, addressCol).Address(0, 0))    ' 写入分割“省”的公式，并将其中的“A2”替换为当前单元格的相对地址，比如“H4”等
                    End If
                    If ws.Cells(i, cityCol).Value = "" Then
                        ws.Cells(i, cityCol).Formula = Replace(formulaCity, "A2", ws.Cells(i, addressCol).Address(0, 0))    ' 写入分割“市”的公式，并将其中的“A2”替换为当前单元格的相对地址，比如“H4”等
                        ws.Cells(i, cityCol + 1).Value = "City is empty"   ' 标记
                    End If

                    ' 分割“街道”列。先判断本次的“详细地址”是否与上一次的“详细地址”相同。若相同则可以直接使用上一次的街道值，尽量减少插入公式或调用python脚本的次数以加快速度；若不同，则依据情况选择插入公式或调用python脚本。
                    ' 查漏补缺。只有“街道”列为空时才进行
                    ' If IsEmpty(ws.Cells(i, townCol)) Then
                    '     If StrComp(addressCellValue, lastaddressCellValue, vbTextCompare) = 0 Then ' 直接使用上一次的街道值
                    '         ' ws.Cells(i, townCol).Value = ws.Cells(i - 1, townCol).Value
                    '         ws.Cells(i, townCol).Value = lastTownValue

                    '         ' ws.Cells(i, townCol + 1).Value = "Cached"

                    '     Else    ' 若不同，则看当前的“详细地址”列的值是否包含“街道”
                    '         If InStr(addressCellValue, "街道") > 0 Then ' 若包含“街道”，则写入公式分割地点
                    '             ws.Cells(i, townCol).Formula = Replace(formulaTown, "A2", ws.Cells(i, addressCol).Address(0, 0))    ' 写入分割“街道”的公式，并将其中的“A2”替换为当前单元格的相对地址，比如“H4”等

                    '             ' ws.Cells(i, townCol + 1).Value = "Formulated"

                    '         Else    ' 若不包含“街道”，则调用python脚本获取街道信息并填入
                    '             If (IsEmpty(ws.Cells(i, latitudeCol)) <> True) And (IsEmpty(ws.Cells(i, longitudeCol)) <> True) Then ' 如果经度和纬度列均不为空，则调用 getTown_GD_useLongitude.py ，以经纬度来查询
                                    
                    '                 longitudeParam = ws.Cells(i, longitudeCol).Value
                    '                 latitudeParam = ws.Cells(i, latitudeCol).Value

                    '                 ' 拼接命令并执行python脚本，把python脚本输出的结果写入“街道”列中
                    '                 ' shellCmd = "pythonw " & pythonScript & " " & regionParam & " " & queryParam
                    '                 pythonScript = ThisWorkbook.Path & "\getTown_GD_useLongitude.py"
                    '                 shellCmd = "pythonw " & pythonScript & " " & longitudeParam & " " & latitudeParam

                    '                 Set exec = shellObj.exec(shellCmd)
                    '                 result = exec.StdOut.ReadAll
                    '             Else    ' 若经纬度信息有空值，则调用 getTown_GD_useAddress.py ,以地址信息来查询

                    '                 result = ""
                    '             End If
                    '             ' regionParam = ws.Cells(i, cityCol).Value    ' 查询区域设置为“市”那一列的值
                    '             ' queryParam = addressCellValue & locationCellValue     ' 查询关键词设置为“详细地址”列的值拼接上“地点”列的值
              

                    '             ' If Trim(result) = "" Then   ' 若结果为空，则尝试查询关键词只设置为“地点”列的值
                    '             '     queryParam = locationCellValue
                    '             '     shellCmd = "pythonw " & pythonScript & " " & regionParam & " " & queryParam
                    '             '     Set exec = shellObj.exec(shellCmd)
                    '             '     result = exec.StdOut.ReadAll
                    '             ' End If
                    '             ' If Trim(result) = "" Then   ' 若结果为空，则尝试查询关键词只设置为“详细地址”列的值
                    '             '     queryParam = addressCellValue
                    '             '     shellCmd = "pythonw " & pythonScript & " " & regionParam & " " & queryParam
                    '             '     Set exec = shellObj.exec(shellCmd)
                    '             '     result = exec.StdOut.ReadAll
                    '             ' End If
                                

                    '             ws.Cells(i, townCol).Value = result
                    '             Set exec = Nothing
                    '         End If
                    '     End If
                    '     lastaddressCellValue = ws.Cells(i, addressCol).Value   ' 更新上一次的“详细地址”值为当前的“详细地址”值
                    '     lastTownValue = ws.Cells(i, townCol).Value
                    '     wb.Save
                    ' End If
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