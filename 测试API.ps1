# 测试腾讯财经API
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "测试腾讯财经 API" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$stockCodes = @("sz000001", "sh600036", "sz302132")

foreach ($code in $stockCodes) {
    Write-Host "测试股票代码: $code" -ForegroundColor Yellow
    
    try {
        $url = "https://qt.gtimg.cn/q=$code&_t=" + [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()
        Write-Host "请求URL: $url" -ForegroundColor Gray
        
        $response = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 10
        
        if ($response.StatusCode -eq 200) {
            Write-Host "✓ HTTP状态码: 200 (成功)" -ForegroundColor Green
            
            # 转换为GBK编码
            $gbk = [System.Text.Encoding]::GetEncoding("GBK")
            $content = $gbk.GetString($response.Content)
            
            Write-Host "返回数据长度: $($content.Length) 字符" -ForegroundColor Gray
            Write-Host "返回内容: $($content.Substring(0, [Math]::Min(200, $content.Length)))" -ForegroundColor Gray
            
            # 解析数据
            if ($content -match '"(.+?)"') {
                $data = $Matches[1]
                $parts = $data -split '~'
                
                if ($parts.Length -ge 5) {
                    $stockName = $parts[1]
                    $currentPrice = $parts[3]
                    $prevClose = $parts[4]
                    
                    Write-Host "✓ 股票名称: $stockName" -ForegroundColor Green
                    Write-Host "✓ 当前价格: $currentPrice" -ForegroundColor Green
                    Write-Host "✓ 昨收价格: $prevClose" -ForegroundColor Green
                    
                    $change = [decimal]$currentPrice - [decimal]$prevClose
                    $changePercent = ($change / [decimal]$prevClose) * 100
                    Write-Host "✓ 涨跌: $($change.ToString('F2')) ($($changePercent.ToString('F2'))%)" -ForegroundColor $(if ($change -gt 0) { "Green" } elseif ($change -lt 0) { "Red" } else { "White" })
                } else {
                    Write-Host "✗ 数据格式不正确，字段数量: $($parts.Length)" -ForegroundColor Red
                }
            } else {
                Write-Host "✗ 无法解析返回数据" -ForegroundColor Red
            }
        } else {
            Write-Host "✗ HTTP状态码: $($response.StatusCode)" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "✗ 请求失败: $($_.Exception.Message)" -ForegroundColor Red
    }
    
    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "测试完成" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "按任意键退出..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

