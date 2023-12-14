using Microsoft.Extensions.Logging;

namespace LeaveSystem.Shared.Logger;

public static class LoggerExtensions
{
    public static void LogException(this ILogger logger, Exception e)
    {
        logger.LogError("{Message}\n{StackTrace}", e.Message, e.StackTrace);
    }
}