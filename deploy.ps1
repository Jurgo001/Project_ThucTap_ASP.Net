$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
Write-Host "====================================="
Write-Host " ProductCRUD Deployment"
Write-Host "====================================="

Set-Location $PSScriptRoot

# 1. Check Docker
Write-Host "`n[1/6] Checking Docker..."

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Host "ERROR: Docker CLI not found."
    exit 1
}

docker info *> $null

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Docker Engine is not running."
    exit 1
}

Write-Host "Docker is running."

# 2. Check configuration
Write-Host "`n[2/6] Checking configuration..."

if (-not (Test-Path ".\docker-compose.yml")) {
    Write-Host "ERROR: docker-compose.yml not found."
    exit 1
}

if (-not (Test-Path ".\.env")) {
    Write-Host "ERROR: .env not found."
    exit 1
}

docker compose config --quiet

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Invalid Docker Compose configuration."
    exit 1
}

Write-Host "Configuration OK."

# 3. Build images
Write-Host "`n[3/6] Building images..."

docker compose build

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Docker image build failed."
    exit 1
}

Write-Host "Images built successfully."

# 4. Start or update services
Write-Host "`n[4/6] Starting or updating services..."

docker compose up -d --remove-orphans

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to start services."
    exit 1
}

# 5. Wait for all containers
Write-Host "`n[5/6] Waiting for services..."

$requiredServices = @(
    "sqlserver",
    "redis",
    "seq",
    "api",
    "gateway",
    "frontend"
)

$allRunning = $false
$missingServices = @()

for ($attempt = 1; $attempt -le 12; $attempt++) {
    $runningServices = @(
        docker compose ps `
            --services `
            --filter "status=running"
    )

    $missingServices = @(
        $requiredServices |
            Where-Object {
                $_ -notin $runningServices
            }
    )

    if ($missingServices.Count -eq 0) {
        $allRunning = $true
        break
    }

    Write-Host "Waiting for: $($missingServices -join ', ')"
    Start-Sleep -Seconds 5
}

if (-not $allRunning) {
    Write-Host "ERROR: Some services are not running:"
    Write-Host ($missingServices -join ", ")

    docker compose ps

    foreach ($service in $missingServices) {
        Write-Host "`nLogs for ${service}:"
        docker compose logs $service --tail 50
    }

    exit 1
}

Write-Host "All six services are running."

# 6. Verify endpoints
Write-Host "`n[6/6] Verifying endpoints..."

$checksPassed = $true

try {
    $frontend = Invoke-WebRequest `
        -Uri "http://localhost:4200" `
        -UseBasicParsing `
        -TimeoutSec 15

    if ($frontend.StatusCode -eq 200) {
        Write-Host "Frontend OK."
    }
}
catch {
    Write-Host "ERROR: Frontend is not ready."
    $checksPassed = $false
}

$apiReady = $false

for ($attempt = 1; $attempt -le 12; $attempt++) {
    try {
        $api = Invoke-WebRequest `
            -Uri "http://localhost:5081/swagger/index.html" `
            -UseBasicParsing `
            -TimeoutSec 10 `
            -ErrorAction Stop

        if ($api.StatusCode -eq 200) {
            $apiReady = $true
            break
        }
    }
    catch {
        Write-Host "Waiting for API... ($attempt/12)"
    }

    Start-Sleep -Seconds 5
}

if ($apiReady) {
    Write-Host "API OK."
}
else {
    Write-Host "ERROR: API is not ready after 60 seconds."
    $checksPassed = $false
}

$gatewayReady = Test-NetConnection `
    -ComputerName "localhost" `
    -Port 5082 `
    -InformationLevel Quiet `
    -WarningAction SilentlyContinue

if ($gatewayReady) {
    Write-Host "Gateway OK."
}
else {
    Write-Host "ERROR: Gateway is not ready."
    $checksPassed = $false
}

try {
    $seq = Invoke-WebRequest `
        -Uri "http://localhost:5342" `
        -UseBasicParsing `
        -TimeoutSec 15

    if ($seq.StatusCode -eq 200) {
        Write-Host "Seq OK."
    }
}
catch {
    Write-Host "ERROR: Seq is not ready."
    $checksPassed = $false
}

docker compose ps

if (-not $checksPassed) {
    Write-Host "`nDeployment verification failed."
    Write-Host "Inspect logs with:"
    Write-Host "docker compose logs gateway api frontend seq --tail 100"
    exit 1
}

Write-Host ""
Write-Host "====================================="
Write-Host " Deployment finished successfully"
Write-Host "====================================="
Write-Host "Frontend : http://localhost:4200"
Write-Host "Gateway  : http://localhost:5082"
Write-Host "Swagger  : http://localhost:5081/swagger"
Write-Host "Seq      : http://localhost:5342"
Write-Host "SQL      : localhost:1433"
Write-Host "Redis    : localhost:6379"
Write-Host "====================================="