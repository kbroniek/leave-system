using System.Globalization;
using LeaveSystem.Api.Seed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class Program
{
    private static async Task Main(string[] args)
    {
        var configuration = GetConfiguration();
        using var loggerFactory = GetLoggingFactory();
        await configuration.FillInDatabase(
            loggerFactory.CreateLogger<Program>(),
            DateTimeOffset.Parse("2024-02-01 00:00:00 +00:00", CultureInfo.CurrentCulture));


        static IConfigurationRoot GetConfiguration() => new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddUserSecrets("af564b3c-4c1b-4a73-894e-549843ad9a0b")
            .Build();

        static ILoggerFactory GetLoggingFactory() =>
            LoggerFactory.Create(builder =>
                builder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = false;
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                }));
    }
}
