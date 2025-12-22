param(
    [Parameter(Mandatory=$true)]
    [string]$DeploymentToken,

    [Parameter(Mandatory=$false)]
    [string]$Environment = "preview",

    [Parameter(Mandatory=$false)]
    [switch]$VerboseLogs
)

# Remove the existing out\deploy directory if it exists
if (Test-Path -Path "out\deploy") {
    Remove-Item -Path "out\deploy" -Recurse -Force
}

# Create the required directory structure
New-Item -Path "out\deploy\api" -ItemType Directory -Force | Out-Null
New-Item -Path "out\deploy\app" -ItemType Directory -Force | Out-Null

# Deploy the Static Web App using the given deployment token and environment
$deployArgs = @(
    "deploy",
    "--deployment-token", $DeploymentToken,
    "--env", $Environment
)

if ($VerboseLogs) {
    $deployArgs += "--verbose", "silly"
}

& swa $deployArgs
