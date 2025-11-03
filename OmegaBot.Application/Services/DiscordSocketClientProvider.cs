using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using OmegaBot.Services.Interfaces;

namespace OmegaBot.Services
{
    public class DiscordSocketClientProvider : IDiscordSocketClientProvider
    {
        private static DiscordSocketClient _client;

        private readonly IDiscordLogger _discordLogger;

        private readonly AppSettings _appSettings;

        private readonly SemaphoreSlim _semaphore = new(1);

        public DiscordSocketClientProvider(IDiscordLogger discordLogger, AppSettings appSettings)
        {
            _discordLogger = discordLogger;
            _appSettings = appSettings;
        }

        public DiscordSocketClient GetDiscordSocketClient()
        {
            _semaphore.WaitAsync().GetAwaiter().GetResult();

            try
            {
                if (_client == null)
                {
                    InitializeConnection().GetAwaiter().GetResult();
                }
            }
            catch (Exception e)
            {
                _discordLogger.ApplicationLog($"Error while initializing discord socket connection: {e.Message}",
                    LogSeverity.Critical);
            }
            finally
            {
                // Using semaphore makes sure to always release it, even if exception occurs
                _semaphore.Release();
            }

            return _client;
        }

        private async Task InitializeConnection()
        {
            if (_client != null)
            {
                await _client.LogoutAsync();
                await _client.StopAsync();
            }

            var config = new DiscordSocketConfig()
            {
                GatewayIntents =
                    GatewayIntents.AllUnprivileged,
            };

            _client = new DiscordSocketClient(config);

            _client.Log += _discordLogger.LogDiscordMessage;

            await _discordLogger.ApplicationLog("Application starting");
            await _client.LoginAsync(TokenType.Bot, _appSettings.AuthToken);
            await _client.StartAsync();

            // StartAsync works on another thread so we need to wait a moment
            while (_client.ConnectionState != ConnectionState.Connected)
            {
                await Task.Delay(new TimeSpan(0, 0, 0, 1));
            }

            _discordLogger.ApplicationLog("Connection established successfully!").GetAwaiter().GetResult();
        }
    }
}
