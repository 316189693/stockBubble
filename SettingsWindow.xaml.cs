using System.Windows;

namespace StockBubble
{
    public partial class SettingsWindow : Window
    {
        public string StockCode { get; set; }

        public SettingsWindow(string currentStockCode)
        {
            InitializeComponent();
            StockCode = currentStockCode;
            StockCodeTextBox.Text = currentStockCode;
            StockCodeTextBox.Focus();
            StockCodeTextBox.SelectAll();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            StockCode = StockCodeTextBox.Text.Trim();
            
            if (string.IsNullOrEmpty(StockCode))
            {
                MessageBox.Show("请输入代码", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

