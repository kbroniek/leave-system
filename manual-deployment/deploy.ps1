param(
    [Parameter(Mandatory=$true)]
    [string]$DeploymentToken
)

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Definition
$webPath = Join-Path $scriptRoot "..\src\web"
$functionsPath = Join-Path $scriptRoot "..\src\LeaveSystem.Functions\LeaveSystem.Functions"
$deployPath = Join-Path $scriptRoot "..\out\deploy"

# Save original directory location
$originalLocation = Get-Location

try {
    # Build web
    Write-Host "Building web app..." -ForegroundColor Cyan
    Set-Location $webPath
    pnpm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "pnpm install failed!" -ForegroundColor Red
        exit 1
    }

    pnpm run build
    if ($LASTEXITCODE -ne 0) {
        Write-Host "pnpm build failed!" -ForegroundColor Red
        exit 1
    }

    # Build Functions
    Write-Host "Building Functions..." -ForegroundColor Cyan
    Set-Location $functionsPath
    dotnet publish -c Release -o ./bin/Release/net8.0/publish
    if ($LASTEXITCODE -ne 0) {
        Write-Host "dotnet publish failed!" -ForegroundColor Red
        exit 1
    }

    # Prepare deployment folder
    Write-Host "Preparing deployment folder..." -ForegroundColor Cyan
    Remove-Item -Path $deployPath -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $deployPath -Force | Out-Null
    New-Item -ItemType Directory -Path "$deployPath\api" -Force | Out-Null

    # Copy files
    Write-Host "Copying files..." -ForegroundColor Cyan
    Copy-Item -Path "$webPath\dist\*" -Destination $deployPath -Recurse -Force
    Copy-Item -Path "$webPath\staticwebapp.config.json" -Destination $deployPath -Force
    Copy-Item -Path "$functionsPath\bin\Release\net8.0\publish\*" -Destination "$deployPath\api" -Recurse -Force

    Write-Host "Deployment folder ready at: $deployPath" -ForegroundColor Green

    # Deploy to Azure Static Web Apps
    Write-Host "Deploying to Azure Static Web Apps..." -ForegroundColor Cyan
    swa deploy $deployPath --deployment-token $DeploymentToken

    if ($LASTEXITCODE -eq 0) {
        Write-Host "Deployment completed successfully!" -ForegroundColor Green
    } else {
        Write-Host "Deployment failed!" -ForegroundColor Red
        exit 1
    }
}
finally {
    # Restore original directory
    Set-Location $originalLocation
}