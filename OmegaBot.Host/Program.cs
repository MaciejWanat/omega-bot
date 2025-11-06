using Discord;
using OmegaBot.Commands.Background;
using OmegaBot.Commands.Background.BotActions;
using OmegaBot.Services;
using OmegaBot.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using OmegaBot.Application.Services;
using OmegaBot.Application.Services.Interfaces;
using OmegaBot.Infrastructure;

namespace OmegaBot
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("logs/OmegaBot-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 31)
                .WriteTo.Console()
                .CreateLogger();

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment == null)
            {
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
            }

            using var host = CreateHostBuilder(Array.Empty<string>()).Build();
            var appSettings = host.Services.GetRequiredService<AppSettings>();
            var client = host.Services.GetRequiredService<IDiscordSocketClientProvider>().GetDiscordSocketClient();

            while (client.ConnectionState != ConnectionState.Connected)
            {
                await Task.Delay(new TimeSpan(0, 0, 0, 1));
            }
            
            var guild = client.Guilds.Single(g => g.Id == appSettings.GuildId);

            while (!guild.IsConnected)
            {
                await Task.Delay(new TimeSpan(0, 0, 0, 1));
            }

            await host.RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

                    var builder = new ConfigurationBuilder()
                        .AddJsonFile(
                            path: "appsettings.json",
                            optional: false,
                            reloadOnChange: true)
                        .AddJsonFile(path: $"appsettings.{environment}.json",
                            optional: true,
                            reloadOnChange: true);

                    var config = builder.Build();

                    services
                        .AddSingleton(config.Get<AppSettings>())
                        .AddSingleton<IDishOfDayRepository, DishOfDayRepository>()
                        .AddSingleton<IDiscordLogger, DiscordLogger>()
                        .AddSingleton<IDiscordSocketClientProvider, DiscordSocketClientProvider>()
                        .AddSingleton<IRssService, RssService>()
                        .AddSingleton<FetchMealOfTheDayAction>()
                        .AddHostedService<BackgroundActionRunner<FetchMealOfTheDayAction>>()
                        .AddHttpClient();
                })
                .UseSerilog()
                .UseEnvironment(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
    }
}
