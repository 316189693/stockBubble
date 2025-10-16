using System;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StockBubble
{
    public class StockData
    {
        public decimal Price { get; set; }
        public decimal PreviousClose { get; set; }
    }

    public class StockService
    {
        private readonly HttpClient _httpClient;

        public StockService()
        {
            // 注册编码提供程序以支持GBK
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        public async Task<StockData?> GetStockPriceAsync(string stockCode)
        {
            try
            {
                // 验证代码格式
                if (string.IsNullOrWhiteSpace(stockCode))
                {
                    throw new Exception("代码不能为空");
                }

                if (!Regex.IsMatch(stockCode, @"^(sz|sh)\d{6}$", RegexOptions.IgnoreCase))
                {
                    throw new Exception($"代码格式错误: {stockCode}\n正确格式: sz302132 或 sh600745");
                }

                // 使用腾讯财经API
                var url = $"https://qt.gtimg.cn/q={stockCode}&_t={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
                
                HttpResponseMessage response;
                try
                {
                    response = await _httpClient.GetAsync(url);
                }
                catch (HttpRequestException ex)
                {
                    throw new Exception($"网络请求失败: {ex.Message}\n请检查网络连接", ex);
                }
                catch (TaskCanceledException)
                {
                    throw new Exception("请求超时\n请检查网络连接");
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"服务器返回错误: HTTP {(int)response.StatusCode}");
                }
                
                // 读取字节数组并使用GBK解码
                var bytes = await response.Content.ReadAsByteArrayAsync();
                
                if (bytes.Length == 0)
                {
                    throw new Exception("服务器返回空数据");
                }
                
                var text = Encoding.GetEncoding("GBK").GetString(bytes);
                
                // 解析数据: v_sz302132="51~华大九天~302132~...
                var match = Regex.Match(text, "\"(.+?)\"");
                if (!match.Success)
                {
                    throw new Exception($"数据解析失败\n返回内容: {(text.Length > 50 ? text.Substring(0, 50) + "..." : text)}");
                }
                
                var data = match.Groups[1].Value;
                var parts = data.Split('~');
                
                if (parts.Length < 5)
                {
                    throw new Exception($"数据格式不正确\n可能是代码错误: {stockCode}\n请检查代码是否正确");
                }
                
                // parts[3] 是当前价格
                // parts[4] 是昨收价
                if (!decimal.TryParse(parts[3], out var price))
                {
                    throw new Exception($"价格数据无效: {parts[3]}\n可能是市场未开盘或代码错误");
                }
                
                if (!decimal.TryParse(parts[4], out var prevClose))
                {
                    throw new Exception($"昨收价数据无效: {parts[4]}");
                }

                // 检查价格是否有效（大于0）
                if (price <= 0 || prevClose <= 0)
                {
                    throw new Exception($"代码可能错误或已停牌: {stockCode}\n当前价: {price}, 昨收: {prevClose}");
                }
                
                return new StockData
                {
                    Price = price,
                    PreviousClose = prevClose
                };
            }
            catch (Exception ex)
            {
                // 重新抛出异常，让调用方处理
                throw new Exception($"获取数据失败: {ex.Message}", ex);
            }
        }
    }
}

