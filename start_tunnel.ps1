param(
    [switch]$NoBrowser
)

$Host.UI.RawUI.WindowTitle = "MiniStore 小店系统 & 公网穿透"

Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "  MiniStore 小店预约系统 - 一键启动与公网穿透     " -ForegroundColor Yellow
Write-Host "=================================================" -ForegroundColor Cyan

$cloudflaredPath = "C:\Program Files (x86)\cloudflared\cloudflared.exe"

# 1. 检查 cloudflared 是否已安装
if (-not (Test-Path $cloudflaredPath)) {
    Write-Host "[1/3] 正在安装 Cloudflare Tunnel 穿透组件..." -ForegroundColor Yellow
    winget install --id Cloudflare.cloudflared --silent --accept-package-agreements --accept-source-agreements
}

# 2. 检查 .NET 后端服务是否已在 5033 端口运行
Write-Host "[2/3] 检查 MiniStore 后端服务 (端口 5033)..." -ForegroundColor Cyan
$backendOnline = $false
try {
    $tcp = New-Object System.Net.Sockets.TcpClient
    $iar = $tcp.BeginConnect("127.0.0.1", 5033, $null, $null)
    $success = $iar.AsyncWaitHandle.WaitOne(800, $false)
    if ($success -and $tcp.Connected) {
        $tcp.EndConnect($iar)
        $backendOnline = $true
    }
    $tcp.Close()
} catch {
    $backendOnline = $false
}

if (-not $backendOnline) {
    Write-Host "  -> 检测到后端服务未启动，正在自动启动 .NET 后端..." -ForegroundColor Yellow
    $apiProjectDir = Join-Path $PSScriptRoot "src\MiniStore.Api"
    
    # 在独立窗口启动后端，避免阻塞当前控制台
    Start-Process -FilePath "dotnet" `
        -ArgumentList "run --project `"$apiProjectDir`" --launch-profile http" `
        -WorkingDirectory $PSScriptRoot `
        -WindowStyle Normal

    Write-Host "  -> 正在等待后端启动就绪 (最多等待 20 秒)..." -ForegroundColor Gray
    for ($i = 1; $i -le 20; $i++) {
        Start-Sleep -Seconds 1
        try {
            $tcp = New-Object System.Net.Sockets.TcpClient
            $iar = $tcp.BeginConnect("127.0.0.1", 5033, $null, $null)
            $success = $iar.AsyncWaitHandle.WaitOne(600, $false)
            if ($success -and $tcp.Connected) {
                $tcp.EndConnect($iar)
                $tcp.Close()
                $backendOnline = $true
                break
            }
            $tcp.Close()
        } catch { }
    }

    if ($backendOnline) {
        Write-Host "  -> 后端服务已成功就绪！(http://127.0.0.1:5033)" -ForegroundColor Green
    } else {
        Write-Host "  -> 后端服务启动耗时较长，请检查新弹出的 dotnet 窗口是否报错。" -ForegroundColor Red
    }
} else {
    Write-Host "  -> 后端服务已在正常运行中 (http://127.0.0.1:5033)" -ForegroundColor Green
}

# 3. 启动 Cloudflare Tunnel（强制绑定 127.0.0.1 避免 IPv6 [::1] 拒绝连接）
Write-Host "[3/3] 正在启动 Cloudflare 公网隧道..." -ForegroundColor Green
Write-Host "-------------------------------------------------" -ForegroundColor Gray
Write-Host "注意：控制台下方输出包含 *.trycloudflare.com 的网址即为公网访问地址" -ForegroundColor Yellow
Write-Host "示例：https://xxxx.trycloudflare.com/s/muse-nail (顾客端)" -ForegroundColor Cyan
Write-Host "示例：https://xxxx.trycloudflare.com/admin      (管理后台)" -ForegroundColor Cyan
Write-Host "按 Ctrl + C 可随时停止穿透服务" -ForegroundColor Gray
Write-Host "-------------------------------------------------" -ForegroundColor Gray

& $cloudflaredPath tunnel --url http://127.0.0.1:5033
