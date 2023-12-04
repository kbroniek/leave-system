using Microsoft.AspNetCore.Components;

namespace LeaveSystem.Web.Extensions;

public static class NavigationManagerExtensions
{
    public static void ReloadPage(this NavigationManager manager)
    {
        manager.NavigateTo(manager.Uri, true);
    }
}
