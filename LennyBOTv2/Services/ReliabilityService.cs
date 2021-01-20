using System;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace LennyBOTv2.Services
{
    /// <summary>
    /// <para>
    /// This service requires that your bot is being run by a daemon that handles
    /// Exit Code 1 (or any exit code) as a restart.
    /// </para>
    /// <para>
    /// If you do not have your bot setup to run in a daemon, this service will just
    /// terminate the process and the bot will not restart.
    /// </para>
    /// </summary>
    /// <remarks>
    /// By foxbot https://gist.github.com/foxbot/7d81edab4e36497c643828638af289b8
    /// Links to daemons:
    /// [Powershell (Windows+Unix)] https://gitlab.com/snippets/21444
    /// [Bash (Unix)] https://stackoverflow.com/a/697064
    /// </remarks>
    public class ReliabilityService
    {
        // Should we attempt to reset the client? Set this to false if your client is still locking up.
        private const bool attemptReset = true;

        private const string logSource = nameof(ReliabilityService);

        private static readonly TimeSpan timeout = TimeSpan.FromSeconds(30);
        private readonly DiscordSocketClient client;
        private CancellationTokenSource token;

        public ReliabilityService(DiscordSocketClient discord)
        {
            token = new CancellationTokenSource();
            client = discord;

            client.Connected += ConnectedAsync;
            client.Disconnected += DisconnectedAsync;
        }

        private static Task LogCriticalAsync(string message)
            => LoggingService.LogCriticalAsync(message, logSource);

        private static Task LogDebugAsync(string message)
            => LoggingService.LogDebugAsync(message, logSource);

        private static Task LogInfoAsync(string message)
            => LoggingService.LogInfoAsync(message, logSource);

        private Task ConnectedAsync()
        {
            // Cancel all previous state checks and reset the CancelToken - client is back online
            LogDebugAsync("Client reconnected, resetting cancel tokens...");
            token.Cancel();
            token = new CancellationTokenSource();
            LogDebugAsync("Client reconnected, cancel tokens reset.");

            return Task.CompletedTask;
        }

        private Task DisconnectedAsync(Exception ex)
        {
            // Check the state after <timeout> to see if we reconnected
            LogInfoAsync("Client disconnected, starting timeout task...");
            Task.Delay(timeout, token.Token).ContinueWith(async _ =>
            {
                await LogDebugAsync("Timeout expired, continuing to check client state...").ConfigureAwait(false);
                await CheckStateAsync().ConfigureAwait(false);
                await LogDebugAsync("State came back okay").ConfigureAwait(false);
            });

            return Task.CompletedTask;
        }

        private async Task CheckStateAsync()
        {
            // Client reconnected, no need to reset
            if (client.ConnectionState == Discord.ConnectionState.Connected) return;
            if (attemptReset)
            {
                await LogInfoAsync("Attempting to reset the client").ConfigureAwait(false);

                var timeout = Task.Delay(ReliabilityService.timeout);
                var connect = client.StartAsync();
                var task = await Task.WhenAny(timeout, connect).ConfigureAwait(false);

                if (task == timeout)
                {
                    await LogCriticalAsync("Client reset timed out (task deadlocked?), killing process").ConfigureAwait(false);
                    Environment.Exit(1);
                }
                else if (connect.IsFaulted)
                {
                    await LoggingService.LogExceptionAsync(connect.Exception, logSource, "Client reset faulted, killing process").ConfigureAwait(false);
                    Environment.Exit(1);
                }
                else if (connect.IsCompletedSuccessfully)
                {
                    await LogInfoAsync("Client reset succesfully!").ConfigureAwait(false);
                }

                return;
            }

#pragma warning disable CS0162 // Unreachable code detected
            await LogCriticalAsync("Client did not reconnect in time, killing process").ConfigureAwait(false);
            Environment.Exit(1);
#pragma warning restore CS0162 // Unreachable code detected
        }
    }
}
