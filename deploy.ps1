$ErrorActionPreference = "Stop"

Write-Host "====================================="
Write-Host " ProductCRUD Fast Deployment"
Write-Host "====================================="

Set-Location $PSScriptRoot

# 1. Check Docker
Write-Host "`n[1/6] Checking Docker..."

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Host "Docker CLI not found."
    exit 1
}

try {
    docker info *> $null
}
catch {
    Write-Host "Docker Engine is not running."
    Write-Host "Please start Docker Desktop first."
    exit 1
}

Write-Host "Docker is running."

# 2. Check configuration
Write-Host "`n[2/6] Checking configuration..."

if (-not (Test-Path ".\docker-compose.yml")) {
    Write-Host "docker-compose.yml not found."
    exit 1
}

if (-not (Test-Path ".\.env")) {
    Write-Host ".env not found."
    exit 1
}

Write-Host "Configuration OK."

# 3. BUILD TRƯỚC
# Không down container ngay để hệ thống cũ vẫn chạy trong lúc build
Write-Host "`n[3/6] Building changed images using Docker cache..."

docker compose build

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed."
    exit 1
}

# 4. Stop old containers AFTER build
Write-Host "`n[4/6] Replacing old containers..."

docker compose down --remove-orphans

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to stop old containers."
    exit 1
}

# 5. Start
Write-Host "`n[5/6] Starting services..."

docker compose up -d

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to start services."
    exit 1
}

Write-Host "Waiting for services..."
Start-Sleep -Seconds 8

# 6. Verify
Write-Host "`n[6/6] Checking containers..."

docker compose ps

Write-Host "`nChecking frontend..."

try {
    $frontend = Invoke-WebRequest `
        -Uri "http://localhost:4200" `
        -UseBasicParsing `
        -TimeoutSec 10

    if ($frontend.StatusCode -eq 200) {
        Write-Host "Frontend OK."
    }
}
catch {
    Write-Host "WARNING: Frontend is not ready yet."
}

Write-Host "`nChecking API..."

try {
    $api = Invoke-WebRequest `
        -Uri "http://localhost:5081/swagger/index.html" `
        -UseBasicParsing `
        -TimeoutSec 10

    if ($api.StatusCode -eq 200) {
        Write-Host "API OK."
    }
}
catch {
    Write-Host "WARNING: API is not ready."
    Write-Host "Run: docker compose logs api --tail 100"
}

Write-Host ""
Write-Host "====================================="
Write-Host " Deployment finished"
Write-Host "====================================="
Write-Host "Frontend : http://localhost:4200"
Write-Host "Swagger  : http://localhost:5081/swagger"
Write-Host "SQL      : localhost:1433"
Write-Host "Redis    : localhost:6379"
Write-Host "====================================="