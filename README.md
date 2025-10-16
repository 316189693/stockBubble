# 股票气泡桌面小挂件

一个独立的 Windows 桌面小挂件，用于实时显示股票价格。气泡悬浮在桌面上，始终保持在最前面。

## 功能特性

- ✨ **桌面悬浮气泡**：独立于浏览器，直接显示在 Windows 桌面
- 🔝 **始终置顶**：气泡窗口始终显示在其他窗口之上
- 🎯 **可拖动**：左键拖动气泡到任意位置
- 🔄 **自动刷新**：每 1 分钟自动更新股票价格
- 📊 **涨跌显示**：
  - 🟢 绿色边框：价格上涨
  - 🔴 红色边框：价格下跌
  - ⚪ 白色边框：价格持平
- ⚙️ **简单配置**：右键菜单轻松设置股票代码

## 系统要求

- Windows 10/11
- .NET 8.0 Runtime

## 快速开始

### 编译运行

1. 确保已安装 [.NET 8.0 SDK](https://dotnet.microsoft.com/download)

2. 打开命令提示符或 PowerShell，导航到项目目录：
   ```bash
   cd C:\Users\willz\Desktop\extensions\stuckbubble
   ```

3. 还原依赖并编译项目：
   ```bash
   dotnet restore
   dotnet build
   ```

4. 运行应用程序：
   ```bash
   dotnet run
   ```

### 发布独立可执行文件

创建单文件可执行程序（无需安装 .NET Runtime）：

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

生成的 exe 文件位于：`bin\Release\net8.0-windows\win-x64\publish\StockBubble.exe`

## 使用说明

### 首次使用

1. 启动应用后，会自动弹出设置窗口
2. 输入股票代码，例如：
   - `sz302132` - 深圳股票（华大九天）
   - `sh600745` - 上海股票（闻泰科技）
3. 点击"确定"，气泡会立即显示股票价格

### 日常使用

- **拖动气泡**：左键按住气泡拖动到任意位置
- **右键菜单**：
  - 设置股票代码 - 更改要监控的股票
  - 刷新 - 立即更新价格
  - 退出 - 关闭应用

### 配置文件

股票代码保存在：`%AppData%\StockBubble\config.txt`

## 股票代码格式

- **深圳股票**：`sz` + 6位代码（如 `sz302132`）
- **上海股票**：`sh` + 6位代码（如 `sh600745`）

## 数据来源

股票数据来自腾讯财经 API（`https://qt.gtimg.cn/`）

## 技术栈

- .NET 8.0
- WPF (Windows Presentation Foundation)
- C#

## 项目结构

```
stuckbubble/
├── StockBubble.csproj      # 项目配置文件
├── App.xaml                # 应用程序入口
├── App.xaml.cs
├── MainWindow.xaml         # 主窗口（气泡）
├── MainWindow.xaml.cs
├── SettingsWindow.xaml     # 设置窗口
├── SettingsWindow.xaml.cs
├── StockService.cs         # 股票数据服务
└── README.md               # 说明文档
```

## 对比原浏览器扩展

| 特性 | 浏览器扩展 | 桌面小挂件 |
|------|-----------|-----------|
| 独立性 | ❌ 依赖浏览器 | ✅ 完全独立 |
| 桌面显示 | ❌ 仅在网页中 | ✅ 桌面任意位置 |
| 资源占用 | 中等 | 低 |
| 启动方式 | 打开浏览器 | 直接运行 exe |
| 系统集成 | 较差 | 良好 |

## 许可证

MIT License

## 贡献

欢迎提交 Issue 和 Pull Request！

