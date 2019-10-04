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
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }

            var message = msg.Message ?? "";
            var exception = !(msg.Exception is null) ? msg.Exception.ToString() : "";

            Console.WriteLine($"{DateTime.Now,-19} [{msg.Severity,8}] {msg.Source}: {message} {exception}");
            Console.ForegroundColor = cc;

            return Task.CompletedTask;
        }

        public static Task LogCriticalAsync(string msg, string source = "") => LogAsync(new LogMessage(LogSeverity.Critical, source, msg));

        public static Task LogDebugAsync(string msg, string source = "") => LogAsync(new LogMessage(LogSeverity.Debug, source, msg));

        public static Task LogErrorAsync(string msg, string source = "") => LogAsync(new LogMessage(LogSeverity.Error, source, msg));

        public static Task LogExceptionAsync(Exception? ex, string source = "", string msg = "") => LogAsync(new LogMessage(LogSeverity.Error, source, msg, ex));

        public static Task LogInfoAsync(string msg, string source = "") => LogAsync(new LogMessage(LogSeverity.Info, source, msg));

        public static Task LogVerboseAsync(string msg, string source = "") => LogAsync(new LogMessage(LogSeverity.Verbose, source, msg));

        public static Task LogWarningAsync(string msg, string source = "") => LogAsync(new LogMessage(LogSeverity.Warning, source, msg));
    }
}