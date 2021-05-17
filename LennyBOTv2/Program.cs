using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LennyBOTv2.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LennyBOTv2
{
    internal class Program
    {
        public static bool IsDebug = false;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IConfiguration _config;
        private readonly string _prefix;
        private IServiceProvider? _services;
        private MessageHandlingService? _messageHandlingService;

        public Program()
        {
#if DEBUG
            IsDebug = true;
#endif
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = IsDebug ? LogSeverity.Debug : LogSeverity.Info,
                MessageCacheSize = 1000,
                AlwaysDownloadUsers = true,
            });
            _commands = new CommandService(new CommandServiceConfig
            {
                DefaultRunMode = RunMode.Async,
                CaseSensitiveCommands = false,
                LogLevel = IsDebug ? LogSeverity.Debug : LogSeverity.Info,
            });
            _config = BuildConfig();
            _prefix = _config["prefix"];
        }

        public async Task MainAsync()
        {
            _client.Log += LoggingService.LogAsync;
            _commands.Log += LoggingService.LogAsync;
            _client.Disconnected += (ex) => LoggingService.LogExceptionAsync(ex, "Client");

            await _client.LoginAsync(TokenType.Bot, _config["token"]).ConfigureAwait(false);
            await _client.StartAsync().ConfigureAwait(false);

            _services = LennyServiceProvider.Instance.Build(_client, _config, _commands);
            _messageHandlingService = _services.GetRequiredService<MessageHandlingService>();

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services).ConfigureAwait(false);
            _client.MessageReceived += MessageReceived;
            _client.MessageDeleted += _messageHandlingService.MessageDeleted;

            await Task.Delay(-1).ConfigureAwait(false);
        }

        private static void Main()
        => new Program().MainAsync().GetAwaiter().GetResult();

        private IConfiguration BuildConfig()
        {
            var cwd = Directory.GetCurrentDirectory();
            var file = IsDebug ? "Files/testConfig.json" : "Files/config.json";
            var path = Path.Combine(cwd, file);
            if (!File.Exists(path))
                throw new FileNotFoundException("Couldn't find config file.", path);
            LoggingService.LogInfoAsync($"Loading {path}", "Config");
            return new ConfigurationBuilder().SetBasePath(cwd).AddJsonFile(file).Build();
        }

        private async Task MessageReceived(SocketMessage rawMessage)
        {
            await _messageHandlingService!.MessageReceived(rawMessage).ConfigureAwait(false);

            // Ignore system messages and messages from bots
            if (rawMessage is not SocketUserMessage message) return;
            if (message.Source != MessageSource.User) return;

            var context = new SocketCommandContext(_client, message);

            if (_messageHandlingService.CheckForRepetition(message))
            {
                await context.Channel.SendMessageAsync(message.Content).ConfigureAwait(false);
            }

            if (message.Channel is IDMChannel dmChannel)
            {
                await _messageHandlingService.LogDMMessageAsync(message).ConfigureAwait(false);
            }

            var argPos = 0;
            if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasStringPrefix(_prefix, ref argPos))) return;

            ////ignore msg with prefix only - disabled as per request
            //if (string.IsNullOrEmpty(message.Content?.Replace(prefix, "").Trim())) return;

            var result = await _commands.ExecuteAsync(context, argPos, _services).ConfigureAwait(false);

            if (result.Error.HasValue)
            {
                switch (result.Error.Value)
                {
                    case CommandError.UnknownCommand:
                        await context.Message.AddReactionAsync(new Emoji("❓")).ConfigureAwait(false);
                        break;

                    case CommandError.Exception:
                        //await LoggingService.LogException(((ExecuteResult)result).Exception);
                        await message.AddReactionAsync(new Emoji("❗")).ConfigureAwait(false);
                        break;

                    case CommandError.ParseFailed:
                    case CommandError.BadArgCount:
                    case CommandError.ObjectNotFound:
                    case CommandError.MultipleMatches:
                    case CommandError.UnmetPrecondition:
                    case CommandError.Unsuccessful:
                    default:
                        await context.MarkCmdFailedAsync(result.ErrorReason).ConfigureAwait(false);
                        break;
                }
            }
        }
    }
}
