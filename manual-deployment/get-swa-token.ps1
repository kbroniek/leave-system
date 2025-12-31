param(
    [Parameter(Mandatory = $true)]
    [string]$StaticWebAppName,

    [Parameter(Mandatory = $true)]
    [string]$ResourceGroupName,

    [Parameter(Mandatory = $false)]
    [string]$TenantId
)

# Login to Azure
Write-Host "Logging into Azure..." -ForegroundColor Cyan

if ($TenantId) {
    az login --tenant $TenantId
} else {
    az login
}

if ($LASTEXITCODE -ne 0) {
    Write-Host "Azure login failed!" -ForegroundColor Red
    exit 1
}

# Get the deployment token
Write-Host "Retrieving deployment token..." -ForegroundColor Cyan
az staticwebapp secrets list `
    --name $StaticWebAppName `
    --resource-group $ResourceGroupName `
    --query "properties.apiKey" -o tsv