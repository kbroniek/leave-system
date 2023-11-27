# Leave system

[![status](https://github.com/kbroniek/leave-system/actions/workflows/dotnet.yml/badge.svg)](https://github.com/kbroniek/leave-system/actions/workflows/dotnet.yml)

System to manage leaves.

## How to run a project

### Manual

Install PostgreSQL and set up a database `leave-system` and a user `postgres` with password `Password12!`.

#### Recreate database

Run:

```
cd "C:\Program Files\pgAdmin 4\v5\runtime"
echo '\x \\ DROP DATABASE "leave-system"; CREATE DATABASE "leave-system";' | .\psql.exe -U postgres
```

### Docker

1. Run: `docker-compose down --remove-orphans; docker-compose up -d`
1. Or if you have clear db: `docker-compose down --volumes; docker-compose up -d`
1. Or with profile: `docker-compose -f docker-compose.yml -f docker-compose.bomed.yml up -d`

#### Helpfull scripts

1. Logs: `docker-compose -f docker-compose.yml -f docker-compose.bomed.yml logs -f`
1. Build docker image: `docker build -t leave .`
1. Save docker image: `docker save --output leave.tar leave`
1. Load docker image: `docker load --input leave.tar`
1. Remove all stopped containers, all dangling images, and all unused networks: `docker system prune`
1. Stop all containers `docker container stop $(docker container ls -aq)`

#### Grab logs

1. Run `docker ps` and take the image id.
1. Run `docker logs {image-ID} -f --tail 100` to get last 100 lines of logs.

## Start using the application B2C

1. Clone the project
2. In your Azure create `Azure Active Directory B2C` resource
3. Create an application. In the `Supported account types` choose `Accounts in any identity provider or organizational directory (for authenticating users with user flows)`
4. In the `Redirect URI (recommended)` choose `SPA` : `https://localhost:7174/authentication/login-callback`

![Create an application](./AzureAppRegistrationB2C.png)

5. Update the `ClientId` and `Authority` in the [appsettings.json](./src/LeaveSystem.Web/LeaveSystem.Web/wwwroot/appsettings.json)

![Update appsettings.json](./AzureAppRegistrationB2C-appsettings.png)

### Assign correct permissions to read and update users

You can create a new application or use existing the `Blazor Server AAD B2C` app.

1. Copy client id and tenant id and paste to the `ManageAzureUsers` in the [appsettings.json](./src/LeaveSystem.Api/LeaveSystem.Api/appsettings.json) config file.
1. Generate `client secret`. Go to the app details and click `Certificates & secrets`. Click `New client secret`. Copy secret and paste to the `ManageAzureUsers/Secret` in the `appsettings.json` config file.
1. Go to the `API permissions`, `Add a permission` and add `User.ReadWrite.All`.
1. Click `Grant admin consent for ...`
1. Find Application (client) ID in the App registrations pane in the Azure portal. The app registration is named 'b2c-extensions-app. Do not modify. Used by AADB2C for storing user data.'. Copy clientId and paste to the `B2cExtensionAppClientId`.

Create new user attribute to expose custom attribute to token claims

1. Open `User attributes` and add new custom attribute and name it `Role` and choose `String` as a data type. Put custom description.
1. Go to the `User flows` and open your user flow e.g. `B2C_1_signupsignin`.
1. Open `User attributes` and select `Role` attribute. 
1. Do the same with `Application claims`.

### Custom policy

https://docs.microsoft.com/en-us/azure/active-directory-b2c/custom-policy-overview#prepare-your-environment

## Start using the application B2B

1. Clone the project
2. Open a terminal and move to the directory with the project `cd LeaveSystem\LeaveSystem.Web`
3. Use the [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/authenticate-azure-cli) to log in to Azure `az login --allow-no-subscriptions`

4. Install [msidentity-app-sync tool](https://github.com/AzureAD/microsoft-identity-web/blob/master/tools/app-provisioning-tool/vs2019-16.9-how-to-use.md) using command 
   
   ```
   dotnet tool install -g msidentity-app-sync
   ```

5. Register the application in Azure
   
   ```
   msidentity-app-sync --tenant-id [tenat-id] --username [username]
   ```
   
   E.g.
   
   ```
   msidentity-app-sync --tenant-id 35ac175a-bb23-4b0a-8b7a-e1d55e5630f9 --username kbroniek@tn4yh.onmicrosoft.com
   ```

6. Cleanup `Program.cs` file and remove redundant line:
   
   ```csharp
   options.ProviderOptions.DefaultAccessTokenScopes.Add("User.Read");
   ```

7. Go to Azure and find the registered application named `LeaveSystem.Web`. Open the Authentication tab and click `This app has implicit grant settings enabled. If you are using any of these URIs in a SPA with MSAL.js 2.0, you should migrate URIs.`.
   
   ![Azure app registration](./AzureAppRegistration.png)

8. Migrate all URLs

### Unregister application

Run command

```
msidentity-app-sync --unregister true
```

## Update the Database schema

Working with the database migrations. https://docs.microsoft.com/pl-pl/ef/core/get-started/overview/first-app?tabs=netcore-cli

Prerequisite:

- Run docker and postgress image

Run:

```
dotnet ef -s LeaveSystem.Api\LeaveSystem.Api\ -p LeaveSystem\LeaveSystem\ migrations add InitialCreate
dotnet ef -s LeaveSystem.Api\LeaveSystem.Api\ -p LeaveSystem\LeaveSystem\ database update
```

If you have issues try to install:

```
    <PackageReference Include="Microsoft.EntityFrameworkCore.Relational" Version="6.0.5" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Relational.Design" Version="1.1.6" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="6.0.5">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
```

## Mapster tool

To generate new mapping I use Mapster. 

**Important: Mapster generate code from dll.**

To regenerate mappers run:

```
dotnet msbuild -t:Mapster
```

To clear all generated files run:

```
dotnet msbuild -t:CleanGenerated
```

More info https://github.com/MapsterMapper/Mapster/wiki/Mapster.Tool.

## Generate security token to the azure B2C

`not working, I don't know why. It is for postman`

1. Install Azure CLI.
2. Login to the tenant, e.g.

```
az login -t leavesystem.onmicrosoft.com --allow-no-subscriptions
```

3. Get access token and replace guid your scope in your server app (https://www.schaeflein.net/use-a-cli-to-get-an-access-token-for-your-aad-protected-web-api/)

```
az account get-access-token --resource api://4f24b978-403f-47fe-9cae-52deea03661d
```

## Storage of app secrets

You can find more info in [documentation](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-7.0&tabs=windows)

1. Go to `src\LeaveSystem.Api\LeaveSystem.Api`
2. Create `secrets.json` file with following content:

```json
{
   "AzureAdB2C:Instance": "your instance",
   "AzureAdB2C:Domain": "your domain",
   "AzureAdB2C:ClientId": "your client id",
   "AzureAdB2C:Scopes": "your scopes",
   "AzureAdB2C:SignUpSignInPolicyId": "your policy id",
   "ManageAzureUsers:TenantId": "your tenat id",
   "ManageAzureUsers:ClientId": "your client id",
   "ManageAzureUsers:Scopes": [
      "your scope"
   ],
   "ManageAzureUsers:Secret": "your secret",
   "ManageAzureUsers:B2cExtensionAppClientId": "your app client id", // Find this Application (client) ID in the App registrations pane in the Azure portal. The app registration is named 'b2c-extensions-app. Do not modify. Used by AADB2C for storing user data.'.
   "ManageAzureUsers:DefaultPassword": "your default password",
   "ManageAzureUsers:Issuer": "your issuer"
}
```

3. Save secrets to store:

```
type .\secrets.json | dotnet user-secrets set
```

## Hot reload BLAZOR

If you want to hot reload blazor app you have to run LeaveSystem.App and then run the command:

```
dotnet watch run --project LeaveSystem.Web/LeaveSystem.Web
```
