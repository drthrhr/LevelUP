@echo off

:: 获取日期(startIndex：表示需获取的日期起始位置；count:表示需要获取几个数字) ： %DATE:~startIndex,count%
set year=%DATE:~2,2%
set month=%DATE:~5,2%
set day=%DATE:~8,2%
set prefix=%year%%month%%day%

:: 获得第i个参数：%i，0<=i && i <=参数最大数量。%~1 会移除参数中的双引号
set info=%~1
set message=%prefix% %info%

:: 执行git同步
git add .
git commit -m "%message%"
git push

pause

:: 自动同步LevelUP的脚本。
:: 能够自动获取当前日期，拼接上第一个命令行参数，作为commit信息来提交。