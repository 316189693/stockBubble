using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace StockBubble
{
    public partial class MainWindow : Window
    {
        private readonly StockService _stockService;
        private readonly DispatcherTimer _refreshTimer;
        private readonly DispatcherTimer _marketCheckTimer; // 检查是否进入交易时间
        private readonly string _configPath;
        private readonly string _sizePath;
        private string? _currentStockCode;
        private bool _isResizing = false;
        private bool _isMarketOpen = false;

        public MainWindow()
        {
            InitializeComponent();
            
            _stockService = new StockService();
            
            // 股价刷新定时器（交易时间内使用）
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1) // 每1分钟刷新
            };
            _refreshTimer.Tick += RefreshTimer_Tick;
            
            // 市场状态检查定时器（非交易时间检查是否开市）
            _marketCheckTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(5) // 每5分钟检查一次
            };
            _marketCheckTimer.Tick += MarketCheckTimer_Tick;
            
            // 配置文件路径
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appData, "StockBubble");
            Directory.CreateDirectory(appFolder);
            _configPath = Path.Combine(appFolder, "config.txt");
            _sizePath = Path.Combine(appFolder, "size.txt");
            
            // 加载保存的窗口大小
            LoadWindowSize();
            
            // 设置窗口初始位置（右上角）
            Left = SystemParameters.WorkArea.Right - Width - 10;
            Top = 10;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 加载保存的代码
            LoadStockCode();
            
            if (!string.IsNullOrEmpty(_currentStockCode))
            {
                // 检查市场状态并刷新
                CheckMarketStatusAndRefresh();
            }
            else
            {
                // 如果没有配置代码，显示设置窗口
                ShowSettings();
            }
        }

        private void LoadStockCode()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    _currentStockCode = File.ReadAllText(_configPath).Trim();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载配置失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveStockCode(string stockCode)
        {
            try
            {
                File.WriteAllText(_configPath, stockCode);
                _currentStockCode = stockCode;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存配置失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task RefreshStockPrice()
        {
            if (string.IsNullOrEmpty(_currentStockCode))
            {
                PriceText.Text = "--";
                ToolTip = "未设置代码\n双击设置";
                return;
            }

            try
            {
                PriceText.Text = "...";
                ToolTip = "正在刷新...";
                
                var stockData = await _stockService.GetStockPriceAsync(_currentStockCode);
                
                if (stockData != null)
                {
                    PriceText.Text = stockData.Price.ToString("F2");
                    
                    // 计算涨跌幅
                    var change = stockData.Price - stockData.PreviousClose;
                    var changePercent = (change / stockData.PreviousClose) * 100;
                    
                    ToolTip = $"代码: {_currentStockCode}\n" +
                             $"当前: {stockData.Price:F2}\n" +
                             $"昨收: {stockData.PreviousClose:F2}\n" +
                             $"涨跌: {change:+0.00;-0.00;0.00} ({changePercent:+0.00;-0.00;0.00}%)\n\n" +
                             $"双击修改代码";
                    
                    // 根据涨跌设置边框颜色（更柔和、更透明）
                    if (stockData.Price > stockData.PreviousClose)
                    {
                        BubbleBorder.Stroke = new SolidColorBrush(Color.FromArgb(180, 76, 175, 80)); // 半透明绿色 - 涨
                        PriceText.Foreground = new SolidColorBrush(Color.FromArgb(220, 56, 142, 60)); // 深绿色文字
                    }
                    else if (stockData.Price < stockData.PreviousClose)
                    {
                        BubbleBorder.Stroke = new SolidColorBrush(Color.FromArgb(180, 244, 67, 54)); // 半透明红色 - 跌
                        PriceText.Foreground = new SolidColorBrush(Color.FromArgb(220, 198, 40, 40)); // 深红色文字
                    }
                    else
                    {
                        BubbleBorder.Stroke = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255)); // 半透明白色 - 平
                        PriceText.Foreground = new SolidColorBrush(Color.FromArgb(220, 0, 0, 0)); // 黑色文字
                    }
                }
                else
                {
                    PriceText.Text = "Err";
                    BubbleBorder.Stroke = new SolidColorBrush(Color.FromArgb(180, 255, 152, 0)); // 半透明橙色
                    PriceText.Foreground = new SolidColorBrush(Color.FromArgb(220, 230, 81, 0)); // 深橙色文字
                    ToolTip = $"代码: {_currentStockCode}\n" +
                             "获取数据失败\n可能原因:\n" +
                             "1. 代码错误\n" +
                             "2. 网络连接问题\n" +
                             "3. API暂时不可用\n\n" +
                             "双击修改代码";
                }
            }
            catch (Exception ex)
            {
                PriceText.Text = "Err";
                BubbleBorder.Stroke = new SolidColorBrush(Color.FromArgb(180, 244, 67, 54)); // 半透明红色
                PriceText.Foreground = new SolidColorBrush(Color.FromArgb(220, 198, 40, 40)); // 深红色文字
                ToolTip = $"代码: {_currentStockCode}\n" +
                         $"错误详情:\n{ex.Message}\n\n" +
                         "可能原因:\n" +
                         "1. 网络连接失败\n" +
                         "2. 防火墙拦截\n" +
                         "3. 代码格式错误\n\n" +
                         "双击修改代码";
                         
                // 记录详细错误到日志
                LogError($"获取数据失败: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void RefreshTimer_Tick(object? sender, EventArgs e)
        {
            // 交易时间刷新股价
            _ = RefreshStockPrice();
        }

        private void MarketCheckTimer_Tick(object? sender, EventArgs e)
        {
            // 检查是否重新进入交易时间
            CheckMarketStatusAndRefresh();
        }

        // 检查市场状态并刷新
        private void CheckMarketStatusAndRefresh()
        {
            _isMarketOpen = IsMarketOpen();
            
            // 先刷新一次价格
            _ = RefreshStockPrice();
            
            if (_isMarketOpen)
            {
                // 交易时间：启动价格刷新定时器，停止市场检查定时器
                if (!_refreshTimer.IsEnabled)
                {
                    _refreshTimer.Start();
                }
                if (_marketCheckTimer.IsEnabled)
                {
                    _marketCheckTimer.Stop();
                }
                
                // 更新提示
                var now = DateTime.Now;
                ToolTip = $"市场开盘中\n当前时间: {now:HH:mm}\n每1分钟自动刷新";
            }
            else
            {
                // 非交易时间：停止价格刷新，启动市场检查定时器
                if (_refreshTimer.IsEnabled)
                {
                    _refreshTimer.Stop();
                }
                if (!_marketCheckTimer.IsEnabled)
                {
                    _marketCheckTimer.Start();
                }
                
                // 更新提示
                var now = DateTime.Now;
                var nextOpen = GetNextMarketOpenTime();
                ToolTip = $"市场休市\n当前时间: {now:HH:mm}\n" +
                         $"下次开盘: {nextOpen}\n" +
                         "显示最新收盘价\n不会自动刷新";
            }
        }

        // 判断当前是否是交易时间
        private bool IsMarketOpen()
        {
            var now = DateTime.Now;
            
            // 周末休市
            if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
            {
                return false;
            }
            
            var time = now.TimeOfDay;
            
            // 上午交易时间：9:30 - 11:30
            if (time >= new TimeSpan(9, 30, 0) && time <= new TimeSpan(11, 30, 0))
            {
                return true;
            }
            
            // 下午交易时间：13:00 - 15:00
            if (time >= new TimeSpan(13, 0, 0) && time <= new TimeSpan(15, 0, 0))
            {
                return true;
            }
            
            return false;
        }

        // 获取下次开盘时间
        private string GetNextMarketOpenTime()
        {
            var now = DateTime.Now;
            var today = now.Date;
            
            // 如果是周五下午3点后，下次开盘是下周一
            if (now.DayOfWeek == DayOfWeek.Friday && now.TimeOfDay > new TimeSpan(15, 0, 0))
            {
                var daysUntilMonday = 3;
                var nextMonday = today.AddDays(daysUntilMonday);
                return $"{nextMonday:MM月dd日} 周一 09:30";
            }
            
            // 如果是周六
            if (now.DayOfWeek == DayOfWeek.Saturday)
            {
                var nextMonday = today.AddDays(2);
                return $"{nextMonday:MM月dd日} 周一 09:30";
            }
            
            // 如果是周日
            if (now.DayOfWeek == DayOfWeek.Sunday)
            {
                var nextMonday = today.AddDays(1);
                return $"{nextMonday:MM月dd日} 周一 09:30";
            }
            
            // 工作日
            var time = now.TimeOfDay;
            
            // 如果在9:30之前，今天9:30开盘
            if (time < new TimeSpan(9, 30, 0))
            {
                return $"今天 09:30";
            }
            
            // 如果在11:30-13:00之间，今天13:00开盘
            if (time > new TimeSpan(11, 30, 0) && time < new TimeSpan(13, 0, 0))
            {
                return $"今天 13:00";
            }
            
            // 如果在15:00之后，明天9:30开盘
            if (time > new TimeSpan(15, 0, 0))
            {
                var tomorrow = today.AddDays(1);
                var dayOfWeek = tomorrow.DayOfWeek;
                
                // 如果明天是周六，下次开盘是下周一
                if (dayOfWeek == DayOfWeek.Saturday)
                {
                    var nextMonday = tomorrow.AddDays(2);
                    return $"{nextMonday:MM月dd日} 周一 09:30";
                }
                
                return $"明天 09:30";
            }
            
            return "未知";
        }

        // 拖动窗口
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1 && e.LeftButton == MouseButtonState.Pressed && !_isResizing)
            {
                try
                {
                    DragMove();
                }
                catch
                {
                    // 忽略拖动异常
                }
            }
        }

        // 双击气泡打开设置
        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                ShowSettings();
            }
        }

        // 鼠标滚轮调整大小
        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var delta = e.Delta > 0 ? 10 : -10;
            var newSize = Math.Max(MinWidth, Math.Min(MaxWidth, Width + delta));
            
            Width = newSize;
            Height = newSize; // 保持正方形
            
            e.Handled = true;
        }

        // 窗口大小改变时保持正方形并保存
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!_isResizing)
            {
                _isResizing = true;
                
                // 保持正方形
                var size = Math.Max(e.NewSize.Width, e.NewSize.Height);
                if (Width != size || Height != size)
                {
                    Width = size;
                    Height = size;
                }
                
                // 保存大小
                SaveWindowSize();
                
                _isResizing = false;
            }
        }

        // 右下角调整大小提示
        private void ResizeGrip_MouseEnter(object sender, MouseEventArgs e)
        {
            ResizeGrip.Opacity = 0.5;
        }

        private void ResizeGrip_MouseLeave(object sender, MouseEventArgs e)
        {
            ResizeGrip.Opacity = 0;
        }

        // 右键菜单会自动显示（由于 XAML 中定义了 ContextMenu）

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            ShowSettings();
        }

        private void ShowSettings()
        {
            var settingsWindow = new SettingsWindow(_currentStockCode ?? "")
            {
                Owner = this
            };
            
            if (settingsWindow.ShowDialog() == true)
            {
                var newStockCode = settingsWindow.StockCode;
                SaveStockCode(newStockCode);
                
                // 检查市场状态并刷新
                CheckMarketStatusAndRefresh();
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            // 手动刷新：重新检查市场状态
            CheckMarketStatusAndRefresh();
        }

        // 快速设置大小
        private void SetSize_Small_Click(object sender, RoutedEventArgs e)
        {
            SetWindowSize(60);
        }

        private void SetSize_Medium_Click(object sender, RoutedEventArgs e)
        {
            SetWindowSize(80);
        }

        private void SetSize_Large_Click(object sender, RoutedEventArgs e)
        {
            SetWindowSize(120);
        }

        private void SetSize_XLarge_Click(object sender, RoutedEventArgs e)
        {
            SetWindowSize(160);
        }

        private void SetWindowSize(double size)
        {
            Width = size;
            Height = size;
            SaveWindowSize();
        }

        private void ViewLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var logPath = Path.Combine(Path.GetDirectoryName(_configPath)!, "error.log");
                
                // 打开日志查看窗口
                var logViewer = new LogViewerWindow(logPath)
                {
                    Owner = this
                };
                logViewer.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法打开日志查看器: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            _refreshTimer.Stop();
            _marketCheckTimer.Stop();
            Application.Current.Shutdown();
        }

        // 加载窗口大小
        private void LoadWindowSize()
        {
            try
            {
                if (File.Exists(_sizePath))
                {
                    var sizeText = File.ReadAllText(_sizePath).Trim();
                    if (double.TryParse(sizeText, out var size))
                    {
                        size = Math.Max(MinWidth, Math.Min(MaxWidth, size));
                        Width = size;
                        Height = size;
                    }
                }
            }
            catch
            {
                // 使用默认大小
            }
        }

        // 保存窗口大小
        private void SaveWindowSize()
        {
            try
            {
                File.WriteAllText(_sizePath, Width.ToString("F0"));
            }
            catch
            {
                // 忽略保存错误
            }
        }

        // 记录错误日志
        private void LogError(string message)
        {
            try
            {
                var logPath = Path.Combine(Path.GetDirectoryName(_configPath)!, "error.log");
                var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n\n";
                File.AppendAllText(logPath, logMessage);
            }
            catch
            {
                // 忽略日志写入错误
            }
        }
    }
}

