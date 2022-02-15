# Leave system

System to manage leaves. 

## Start using the application

1. Clone the project

2. Open a terminal and move to the directory with the project `cd LeaveSystem\LeaveSystem`

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

7. Go to Azure and find the registered application named `LeaveSystem`. Open the Authentication tab and click `This app has implicit grant settings enabled. If you are using any of these URIs in a SPA with MSAL.js 2.0, you should migrate URIs.`.
   
   ![Azure app registration](./AzureAppRegistration.png)

8. Migrate all URLs

## Unregister application

Run command

```
msidentity-app-sync --unregister true
```
