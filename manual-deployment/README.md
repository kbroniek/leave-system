# Manual Deployment Scripts

This folder contains PowerShell scripts for manually deploying the Static Web App to Azure.

## Scripts

### `get-swa-token.ps1`

Retrieves the deployment token for your Azure Static Web App. This token is required for manual deployments.

#### Prerequisites

- Azure CLI installed and configured
- Appropriate permissions to access the Static Web App resource

#### Usage

```powershell
.\get-swa-token.ps1 -StaticWebAppName <your-app-name> -ResourceGroupName <your-resource-group> [-TenantId <tenant-id>]
```

#### Parameters

- `StaticWebAppName` (Required): The name of your Azure Static Web App
- `ResourceGroupName` (Required): The name of the Azure resource group containing the Static Web App
- `TenantId` (Optional): Azure tenant ID if you need to specify a specific tenant

#### Example

```powershell
.\get-swa-token.ps1 -StaticWebAppName "my-leave-system" -ResourceGroupName "rg-leave-system"
```

The script will output the deployment token which you can then use with the `deploy.ps1` script.

---

### `deploy.ps1`

Deploys the Static Web App to Azure using the Azure Static Web Apps CLI.

#### Prerequisites

- Azure Static Web Apps CLI (`swa`) installed
  - Install via: `npm install -g @azure/static-web-apps-cli`
- Deployment token (obtained from `get-swa-token.ps1`)

#### Usage

```powershell
.\deploy.ps1 -DeploymentToken <token> [-Environment <environment>] [-VerboseLogs]
```

#### Parameters

- `DeploymentToken` (Required): The deployment token obtained from `get-swa-token.ps1`
- `Environment` (Optional): The deployment environment. Defaults to `"preview"`
- `VerboseLogs` (Optional): Switch to enable verbose logging

#### Example

```powershell
# Basic deployment to preview environment
.\deploy.ps1 -DeploymentToken "your-deployment-token-here"

# Deploy to production with verbose logs
.\deploy.ps1 -DeploymentToken "your-deployment-token-here" -Environment "production" -VerboseLogs
```

#### What it does

1. Removes the existing `out\deploy` directory if it exists
2. Creates the required directory structure (`out\deploy\api` and `out\deploy\app`)
3. Deploys the Static Web App using the Azure Static Web Apps CLI

---

## Complete Deployment Workflow

1. **Get the deployment token:**

   ```powershell
   $token = .\get-swa-token.ps1 -StaticWebAppName "my-leave-system" -ResourceGroupName "rg-leave-system"
   ```

2. **Deploy the application:**
   ```powershell
   .\deploy.ps1 -DeploymentToken $token -Environment "production"
   ```

---

## Notes

- These scripts are designed for manual deployments when automated CI/CD pipelines are not available or when you need to deploy outside of the normal workflow
- The scripts assume you're running from the project root directory
- Make sure you have the necessary build artifacts in the `out` directory before deploying
- The deployment token is sensitive information - do not commit it to version control
