@echo off
chcp 65001 >nul
title MiniStore 小店系统与公网穿透
echo =======================================================
echo    MiniStore 小店预约系统 - 一键启动器
echo =======================================================
echo 正在检查环境并启动系统，请稍候...
echo.

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0start_tunnel.ps1"

pause
