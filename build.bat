@echo off
chcp 65001 >nul
echo ========================================
echo 股票气泡桌面小挂件 - 编译脚本
echo ========================================
echo.

echo [1/3] 还原 NuGet 包...
dotnet restore
if %errorlevel% neq 0 (
    echo 错误: NuGet 包还原失败
    pause
    exit /b %errorlevel%
)
echo.

echo [2/3] 编译项目...
dotnet build -c Release
if %errorlevel% neq 0 (
    echo 错误: 编译失败
    pause
    exit /b %errorlevel%
)
echo.

echo [3/3] 发布单文件可执行程序...
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
if %errorlevel% neq 0 (
    echo 错误: 发布失败
    pause
    exit /b %errorlevel%
)
echo.

echo ========================================
echo 编译完成！
echo ========================================
echo.
echo 可执行文件位置:
echo bin\Release\net8.0-windows\win-x64\publish\StockBubble.exe
echo.
pause

