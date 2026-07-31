## Build and run the AgilitySports API in Docker.
## Prerequisite: Docker Desktop. For Database:Mode=Docker, also start the V2 SQL stack
## from AgilitySports_Data (.\BuildDockerImage_V2.ps1) so SQL is listening on port 21433.

param(
	[switch]$ForegroundLogs,
	[switch]$NoBuild,
	[switch]$Recreate
)

$ErrorActionPreference = "Stop"

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
	Write-Host "Docker CLI is not installed or not on PATH." -ForegroundColor Red
	Write-Host "Please install Docker Desktop, then run this script again." -ForegroundColor Yellow
	pause
	exit 1
}

docker info *> $null
if ($LASTEXITCODE -ne 0) {
	Write-Host "Docker appears to be stopped." -ForegroundColor Red
	Write-Host "Please start Docker Desktop (or the Docker daemon), wait until it is running, then rerun this script." -ForegroundColor Yellow
	pause
	exit 1
}

$composeDir = Join-Path $PSScriptRoot "Container"
$composeFile = Join-Path $composeDir "docker-compose.yml"
if (-not (Test-Path $composeFile)) {
	Write-Host "Expected compose file not found: $composeFile" -ForegroundColor Red
	pause
	exit 1
}

$envExample = Join-Path $composeDir ".env.example"
$envFile = Join-Path $composeDir ".env"
if (-not (Test-Path $envFile) -and (Test-Path $envExample)) {
	Copy-Item $envExample $envFile
	Write-Host "Created Container\.env from .env.example" -ForegroundColor Cyan
}

Push-Location $composeDir

$resolvedHostApiPort = "1106"
if (-not [string]::IsNullOrWhiteSpace($env:HOST_API_PORT)) {
	$resolvedHostApiPort = $env:HOST_API_PORT.Trim()
}
elseif (Test-Path $envFile) {
	$hostPortLine = Get-Content $envFile | Where-Object { $_ -match '^\s*HOST_API_PORT\s*=' } | Select-Object -First 1
	if ($hostPortLine -and $hostPortLine -match '^\s*HOST_API_PORT\s*=\s*([^#\r\n]+)') {
		$candidatePort = $Matches[1].Trim()
		if (-not [string]::IsNullOrWhiteSpace($candidatePort)) {
			$resolvedHostApiPort = $candidatePort
		}
	}
}

try {
	if ($Recreate) {
		Write-Host "Stopping existing API container..." -ForegroundColor Green
		docker compose down --remove-orphans
		if ($LASTEXITCODE -ne 0) {
			Write-Host "Failed to stop existing compose resources." -ForegroundColor Red
			exit 1
		}
	}

	if ($NoBuild) {
		Write-Host "Starting API container (no rebuild)..." -ForegroundColor Green
		docker compose up -d api
	}
	else {
		Write-Host "Building and starting API container..." -ForegroundColor Green
		docker compose up -d --build api
	}

	if ($LASTEXITCODE -ne 0) {
		Write-Host "Failed to start the API container." -ForegroundColor Red
		exit 1
	}

	Write-Host "AgilitySports API is running at http://localhost:$resolvedHostApiPort" -ForegroundColor Green
	Write-Host "Swagger UI: http://localhost:$resolvedHostApiPort/swagger" -ForegroundColor Green
	Write-Host "Health:    http://localhost:$resolvedHostApiPort/api/v2/checkhealth" -ForegroundColor Green
	Write-Host "DB health: http://localhost:$resolvedHostApiPort/api/v2/health/db" -ForegroundColor DarkGray
	Write-Host "Ensure the V2 SQL container is up (AgilitySports_Data BuildDockerImage_V2.ps1) when Database mode is Docker." -ForegroundColor DarkGray

	if ($ForegroundLogs) {
		Write-Host "Following api logs (Ctrl+C to stop viewing logs)..." -ForegroundColor DarkGray
		docker compose logs -f api
	}
	else {
		Write-Host "Use 'docker compose -f .\Container\docker-compose.yml logs -f api' to view API logs when needed." -ForegroundColor DarkGray
	}
}
finally {
	Pop-Location
}

pause