# Cleanly stops all APE 04 services and frees RAM

Write-Host "=== DETENIENDO TODOS LOS SERVICIOS APE 04 ===" -ForegroundColor Yellow

taskkill /F /IM nginx.exe 2>$null
Get-CimInstance Win32_Process | Where-Object {
    ($_.Name -eq "java.exe" -and ($_.CommandLine -match "venta-vehiculos-api" -or $_.CommandLine -match "notificaciones-service")) -or
    ($_.Name -eq "python.exe" -and $_.CommandLine -match "uvicorn") -or
    ($_.Name -eq "node.exe" -and ($_.CommandLine -match "server\.mjs" -or $_.CommandLine -match "vite")) -or
    ($_.Name -eq "ConsultaVehiculosWinForms.exe")
} | ForEach-Object {
    Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue
    Write-Host "  Detenido: $($_.Name) (PID $($_.ProcessId))" -ForegroundColor Gray
}

Write-Host "Listo. Todos los servicios de APE 04 fueron detenidos (MySQL y RabbitMQ siguen activos si los necesitas)." -ForegroundColor Green
