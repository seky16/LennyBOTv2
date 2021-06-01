using System;
using System.Threading.Tasks;
using Discord;

namespace LennyBOTv2.Services
{
    internal static class LoggingService
    {
        public static Task LogAsync(LogMessage msg)
        {
            var cc = Console.ForegroundColor;
            switch (msg.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;

                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;

                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                default:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }

            Console.WriteLine(msg.ToString());
            Console.ForegroundColor = cc;

            if (msg.Exception is Discord.Net.WebSocketClosedException ||
                msg.Exception is Discord.WebSocket.GatewayReconnectException ||
                msg.Exception.AnyInnerException<System.Net.WebSockets.WebSocketException>())
            {
                Environment.Exit(-1); // reconnect on WebSocket closed
            }

            return Task.CompletedTask;
        }

        public static Task LogCriticalAsync(string? msg, string? source = null) => LogAsync(new LogMessage(LogSeverity.Critical, source ?? string.Empty, msg ?? string.Empty));

        public static Task LogDebugAsync(string? msg, string? source = null) => LogAsync(new LogMessage(LogSeverity.Debug, source ?? string.Empty, msg ?? string.Empty));

        public static Task LogErrorAsync(string? msg, string? source = null) => LogAsync(new LogMessage(LogSeverity.Error, source ?? string.Empty, msg ?? string.Empty));

        public static Task LogExceptionAsync(Exception? ex, string? source = null, string? msg = null) => LogAsync(new LogMessage(LogSeverity.Error, source ?? string.Empty, msg ?? string.Empty, ex));

        public static Task LogInfoAsync(string? msg, string? source = null) => LogAsync(new LogMessage(LogSeverity.Info, source ?? string.Empty, msg ?? string.Empty));

        public static Task LogVerboseAsync(string? msg, string? source = null) => LogAsync(new LogMessage(LogSeverity.Verbose, source ?? string.Empty, msg ?? string.Empty));

        public static Task LogWarningAsync(string? msg, string? source = null) => LogAsync(new LogMessage(LogSeverity.Warning, source ?? string.Empty, msg ?? string.Empty));
    }
}
