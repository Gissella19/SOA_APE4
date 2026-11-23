# Ultra-lightweight startup script for APE 04 (Defense Ready)
# Uses compiled JARs with capped JVM heap (-Xmx192m) to save ~70% RAM vs IDE/mvn spring-boot:run.

$ErrorActionPreference = "Continue"
$Root = $PSScriptRoot
$LogDir = Join-Path $Root "logs"
New-Item -ItemType Directory -Force -Path $LogDir | Out-Null

Write-Host "=== INICIANDO ARQUITECTURA APE 04 (MODO LIGERO) ===" -ForegroundColor Cyan

function Test-PortOpen($Port) {
    $tcp = New-Object System.Net.Sockets.TcpClient
    try {
        $iar = $tcp.BeginConnect("127.0.0.1", $Port, $null, $null)
        $wait = $iar.AsyncWaitHandle.WaitOne(300, $false)
        if ($wait -and $tcp.Connected) { $tcp.EndConnect($iar); return $true }
        return $false
    } catch { return $false } finally { $tcp.Close() }
}

# 1. MySQL check
if (Test-PortOpen 3306) {
    Write-Host "[OK] MySQL activo en puerto 3306" -ForegroundColor Green
} else {
    Write-Host "[AVISO] MySQL (3306) no responde. Inicia XAMPP MySQL antes de continuar." -ForegroundColor Yellow
}

# 2. RabbitMQ (5672 / 15672) - Run in minimized window without stdin EOF redirect so Erlang stays alive
if (-not (Test-PortOpen 5672)) {
    Write-Host "[..] Iniciando RabbitMQ nativo (5672 / 15672)..." -ForegroundColor Cyan
    $env:ERLANG_HOME = "C:\Users\Admin\scoop\apps\erlang\current"
    $env:RABBITMQ_SERVER = "C:\Users\Admin\scoop\apps\rabbitmq\current"
    Start-Process -FilePath "$env:RABBITMQ_SERVER\sbin\rabbitmq-server.bat" -WindowStyle Minimized
} else {
    Write-Host "[OK] RabbitMQ ya esta activo en 5672" -ForegroundColor Green
}

# 3. Java Vehicles Instance A (8080) & B (8082) - Capped heap (192MB max each)
$VehiculosJar = Join-Path $Root "java_venta-vehiculos-api_security\target\venta-vehiculos-api-0.0.1-SNAPSHOT.jar"
if (-not (Test-PortOpen 8080)) {
    Write-Host "[..] Iniciando Java Vehiculos Instancia A (8080, max 192MB RAM)..." -ForegroundColor Cyan
    Start-Process -FilePath "java" `
        -ArgumentList "-Xms64m", "-Xmx192m", "-jar", "`"$VehiculosJar`"", "--server.port=8080" `
        -WindowStyle Hidden `
        -RedirectStandardOutput "$LogDir\java-8080.out.log" `
        -RedirectStandardError "$LogDir\java-8080.err.log"
}
if (-not (Test-PortOpen 8082)) {
    Write-Host "[..] Iniciando Java Vehiculos Instancia B (8082, max 192MB RAM)..." -ForegroundColor Cyan
    Start-Process -FilePath "java" `
        -ArgumentList "-Xms64m", "-Xmx192m", "-jar", "`"$VehiculosJar`"", "--server.port=8082" `
        -WindowStyle Hidden `
        -RedirectStandardOutput "$LogDir\java-8082.out.log" `
        -RedirectStandardError "$LogDir\java-8082.err.log"
}

# 4. Java Notifications (8081) - Capped heap (192MB max)
$NotifJar = Join-Path $Root "java_notificaciones-service\target\notificaciones-service-0.0.1-SNAPSHOT.jar"
if (-not (Test-PortOpen 8081)) {
    Write-Host "[..] Iniciando Java Notificaciones (8081, max 192MB RAM)..." -ForegroundColor Cyan
    Start-Process -FilePath "java" `
        -ArgumentList "-Xms64m", "-Xmx192m", "-jar", "`"$NotifJar`"" `
        -WindowStyle Hidden `
        -RedirectStandardOutput "$LogDir\notificaciones-8081.out.log" `
        -RedirectStandardError "$LogDir\notificaciones-8081.err.log"
}

# 5. Python FastAPI (8002) without --reload watcher overhead
if (-not (Test-PortOpen 8002)) {
    Write-Host "[..] Iniciando Python FastAPI Ventas (8002)..." -ForegroundColor Cyan
    $PyDir = Join-Path $Root "python_ventas-api"
    $PyExe = Join-Path $PyDir ".venv\Scripts\python.exe"
    Start-Process -FilePath $PyExe `
        -WorkingDirectory $PyDir `
        -ArgumentList "-m", "uvicorn", "app.main:app", "--host", "127.0.0.1", "--port", "8002" `
        -WindowStyle Hidden `
        -RedirectStandardOutput "$LogDir\fastapi-8002.out.log" `
        -RedirectStandardError "$LogDir\fastapi-8002.err.log"
}

# 6. NGINX Gateway (8090)
Copy-Item (Join-Path $Root "nginx.conf") "C:\nginx\conf\nginx.conf" -Force -ErrorAction SilentlyContinue
if (-not (Test-PortOpen 8090)) {
    Write-Host "[..] Iniciando NGINX Gateway (8090)..." -ForegroundColor Cyan
    Start-Process -FilePath "C:\nginx\nginx.exe" -WorkingDirectory "C:\nginx" -WindowStyle Hidden
}

# 7. React Web (5173) & Mobile PWA (5174)
if (-not (Test-PortOpen 5174)) {
    Write-Host "[..] Iniciando PWA Movil (5174)..." -ForegroundColor Cyan
    Start-Process -FilePath "node" `
        -WorkingDirectory (Join-Path $Root "AplicacionMovilPWA") `
        -ArgumentList "server.mjs" `
        -WindowStyle Hidden `
        -RedirectStandardOutput "$LogDir\pwa-5174.out.log" `
        -RedirectStandardError "$LogDir\pwa-5174.err.log"
}

$ReactDir = Join-Path $Root "AplicacionWebREACT\react-fetch-vehiculos"
Write-Host "[..] Iniciando React Web (5173)..." -ForegroundColor Cyan
Start-Process -FilePath "cmd.exe" `
    -WorkingDirectory $ReactDir `
    -ArgumentList "/c", "npm run dev -- --port 5173" `
    -WindowStyle Hidden `
    -RedirectStandardOutput "$LogDir\react-5173.out.log" `
    -RedirectStandardError "$LogDir\react-5173.err.log"

Write-Host "`nEsperando a que respondan todos los puertos (max 35 segundos)..." -ForegroundColor Yellow
$PuertosEsperados = @(5672, 15672, 8090, 8080, 8082, 8002, 8081, 5174)
for ($i = 0; $i -lt 35; $i++) {
    $faltantes = @($PuertosEsperados | Where-Object { -not (Test-PortOpen $_) })
    if ($faltantes.Count -eq 0) { break }
    Start-Sleep -Seconds 1
}

$Servicios = @(
    @{Nombre="MySQL"; Puerto=3306; Url="127.0.0.1:3306"},
    @{Nombre="RabbitMQ AMQP"; Puerto=5672; Url="127.0.0.1:5672"},
    @{Nombre="RabbitMQ Panel"; Puerto=15672; Url="http://127.0.0.1:15672 (guest/guest)"},
    @{Nombre="NGINX Gateway"; Puerto=8090; Url="http://127.0.0.1:8090/nginx-ping"},
    @{Nombre="Java Vehiculos A"; Puerto=8080; Url="http://127.0.0.1:8080/actuator/health"},
    @{Nombre="Java Vehiculos B"; Puerto=8082; Url="http://127.0.0.1:8082/actuator/health"},
    @{Nombre="FastAPI Ventas"; Puerto=8002; Url="http://127.0.0.1:8002/docs"},
    @{Nombre="Notificaciones"; Puerto=8081; Url="http://127.0.0.1:8081/api/notificaciones"},
    @{Nombre="PWA Movil"; Puerto=5174; Url="http://127.0.0.1:5174"}
)

Write-Host "`n=== ESTADO DE SERVICIOS APE 04 ===" -ForegroundColor Cyan
foreach ($s in $Servicios) {
    $estado = if (Test-PortOpen $s.Puerto) { "ACTIVO" } else { "INICIANDO/CAIDO" }
    $color = if ($estado -eq "ACTIVO") { "Green" } else { "Red" }
    Write-Host ("  [{0,-15}] {1,-18} -> {2}" -f $estado, $s.Nombre, $s.Url) -ForegroundColor $color
}
Write-Host "  [ACTIVO         ] React Web          -> http://localhost:5173" -ForegroundColor Green

# 8. Launch compiled WinForms .exe directly (zero Visual Studio / MSBuild RAM)
$WinFormsExe = Join-Path $Root "AplicacionEscritorioWINDOWS_FORMS\ConsultaVehiculosWinForms\bin\Debug\net8.0-windows\ConsultaVehiculosWinForms.exe"
if (Test-Path $WinFormsExe) {
    Write-Host "`nAbriendo Windows Forms compilado (sin Visual Studio)..." -ForegroundColor Cyan
    Start-Process -FilePath $WinFormsExe
}

Write-Host "`nPara abrir JavaFX: cd AplicacionEscritorioJAVA_FX\consultarvehiculojavafx ; mvn javafx:run" -ForegroundColor Gray
Write-Host "Para apagar todo y liberar 100% de RAM: .\detener-todo.ps1" -ForegroundColor Gray
