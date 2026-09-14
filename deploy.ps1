$ErrorActionPreference = "Stop"

Write-Host "====================================="
Write-Host " ProductCRUD Deployment"
Write-Host "====================================="

# 1. Đi về đúng thư mục chứa script
Set-Location $PSScriptRoot

# 2. Kiểm tra Docker CLI
Write-Host "`n[1/6] Checking Docker..."

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Host "Docker CLI not found."
    Write-Host "Please install/start Docker Desktop."
    exit 1
}

# 3. Kiểm tra Docker Engine
docker info *> $null

if ($LASTEXITCODE -ne 0) {
    Write-Host "Docker Engine is not running."
    Write-Host "Please start Docker Desktop first."
    exit 1
}

Write-Host "Docker is running."

# 4. Kiểm tra file cần thiết
Write-Host "`n[2/6] Checking configuration..."

if (-not (Test-Path ".\docker-compose.yml")) {
    Write-Host "docker-compose.yml not found."
    exit 1
}

if (-not (Test-Path ".\.env")) {
    Write-Host ".env not found."
    Write-Host "Create .env from .env.example first."
    exit 1
}

Write-Host "Configuration OK."

# 5. Xóa container cũ
Write-Host "`n[3/6] Stopping old containers..."

docker compose down --remove-orphans

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to stop old containers."
    exit 1
}

# 6. Build image mới
Write-Host "`n[4/6] Building images..."

docker compose build

if ($LASTEXITCODE -ne 0) {
    Write-Host "Docker build failed."
    exit 1
}

# 7. Chạy toàn bộ stack
Write-Host "`n[5/6] Starting containers..."

docker compose up -d

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to start containers."
    exit 1
}

# Chờ các service startup
Write-Host "Waiting for services..."
Start-Sleep -Seconds 10

# 8. Hiển thị trạng thái
Write-Host "`n[6/6] Container status:"
docker compose ps

Write-Host ""
Write-Host "====================================="
Write-Host " Deployment completed"
Write-Host "====================================="
Write-Host "Frontend : http://localhost:4200"
Write-Host "Swagger  : http://localhost:5081/swagger"
Write-Host "SQL      : localhost:1433"
Write-Host "Redis    : localhost:6379"
Write-Host "====================================="