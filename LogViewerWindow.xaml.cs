using System;
using System.IO;
using System.Windows;

namespace StockBubble
{
    public partial class LogViewerWindow : Window
    {
        private readonly string _logPath;

        public LogViewerWindow(string logPath)
        {
            InitializeComponent();
            _logPath = logPath;
            
            LogInfoText.Text = $"日志位置: {_logPath}";
            
            Loaded += LogViewerWindow_Loaded;
        }

        private void LogViewerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLog();
        }

        private void LoadLog()
        {
            try
            {
                if (!File.Exists(_logPath))
                {
                    LogTextBox.Text = "暂无日志记录";
                    LogSizeText.Text = "大小: 0 KB";
                    return;
                }

                var fileInfo = new FileInfo(_logPath);
                var sizeKB = fileInfo.Length / 1024.0;
                LogSizeText.Text = $"大小: {sizeKB:F2} KB";

                var content = File.ReadAllText(_logPath);
                
                if (string.IsNullOrWhiteSpace(content))
                {
                    LogTextBox.Text = "日志文件为空";
                }
                else
                {
                    LogTextBox.Text = content;
                    
                    // 自动滚动到底部（显示最新日志）
                    LogScrollViewer.ScrollToEnd();
                }
            }
            catch (Exception ex)
            {
                LogTextBox.Text = $"无法读取日志文件\n\n错误: {ex.Message}";
                LogSizeText.Text = "大小: -- KB";
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadLog();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "确定要清空所有日志记录吗？\n此操作不可恢复。",
                "确认清空",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (File.Exists(_logPath))
                    {
                        File.WriteAllText(_logPath, string.Empty);
                        MessageBox.Show("日志已清空", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadLog();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"清空日志失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

