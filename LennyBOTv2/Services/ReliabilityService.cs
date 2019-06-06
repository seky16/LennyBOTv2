using System;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace LennyBOTv2.Services
{
    public class ReliabilityService
    {
        private readonly DiscordSocketClient client;
        private CancellationTokenSource token;

        public ReliabilityService(DiscordSocketClient discord)
        {
            token = new CancellationTokenSource();
            client = discord;

            client.Connected += ConnectedAsync;
            client.Disconnected += DisconnectedAsync;
        }

        private Task ConnectedAsync()
        {
            LoggingService.LogInfoAsync("Client reconnected");

            // Bot connected so cancel the task.
            token.Cancel();
            token = new CancellationTokenSource();

            return Task.CompletedTask;
        }

        private Task DisconnectedAsync(Exception _e)
        {
            LoggingService.LogErrorAsync($"Client disconnected with error: {_e.Message}");

            Task.Delay(TimeSpan.FromSeconds(15), token.Token).ContinueWith(_ => ResetClient());

            return Task.CompletedTask;
        }

        private void ResetClient()
        {
            // Client reconnected, no need to reset
            if (client.ConnectionState == Discord.ConnectionState.Connected)
                return;

            LoggingService.LogInfoAsync("Attempting to reset the client");

            var connect = client.StartAsync();

            if (connect.IsCompletedSuccessfully)
                LoggingService.LogInfoAsync("Client reset successfully!");
            else
                LoggingService.LogCriticalAsync("Client did not reset successfully");
        }
    }
}