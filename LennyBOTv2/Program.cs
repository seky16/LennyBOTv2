using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LennyBOTv2.Services;
using Microsoft.Extensions.Configuration;

namespace LennyBOTv2
{
    internal class Program
    {
        public static bool IsDebug = false;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IConfiguration _config;
        private IServiceProvider _services;

        public Program()
        {
#if DEBUG
            IsDebug = true;
#endif

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = IsDebug ? LogSeverity.Debug : LogSeverity.Info,
                MessageCacheSize = 1000
            });
            _commands = new CommandService(new CommandServiceConfig
            {
                DefaultRunMode = RunMode.Async,
                CaseSensitiveCommands = false,
                LogLevel = IsDebug ? LogSeverity.Debug : LogSeverity.Info,
            });
            _config = BuildConfig();
        }

        public async Task MainAsync()
        {
            _client.Log += LoggingService.LogAsync;
            _commands.Log += LoggingService.LogAsync;
            _client.Disconnected += (ex) => LoggingService.LogExceptionAsync(ex, "Client");

            await _client.LoginAsync(TokenType.Bot, _config["token"]).ConfigureAwait(false);
            await _client.StartAsync().ConfigureAwait(false);

            _services = LennyServiceProvider.Instance.Build(_client, _config, _commands);

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services).ConfigureAwait(false);
            _client.MessageReceived += HandleMessage;

            await Task.Delay(-1).ConfigureAwait(false);
        }

        private static void Main()
        => new Program().MainAsync().GetAwaiter().GetResult();

        private IConfiguration BuildConfig()
        {
            var cwd = Directory.GetCurrentDirectory();
            var file = IsDebug ? "Files/testConfig.json" : "Files/config.json";
            if (!File.Exists(Path.Combine(cwd, file)))
                throw new FileNotFoundException("Couldn't find config file.", file);
            LoggingService.LogInfoAsync($"{file} loaded", "Config");
            return new ConfigurationBuilder().SetBasePath(cwd).AddJsonFile(file).Build();
        }

        private async Task HandleMessage(SocketMessage rawMessage)
        {
            // Ignore system messages and messages from bots
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            var context = new SocketCommandContext(_client, message);

            if (CheckForRepetition(message))
            {
                await context.Channel.SendMessageAsync(message.Content).ConfigureAwait(false);
                return;
            }

            var prefix = _config["prefix"];
            int argPos = 0;
            if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasStringPrefix(prefix, ref argPos))) return;

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

        #region Repetition

        private readonly List<SocketUserMessage> lastMessages = new List<SocketUserMessage>();
        private const int repetitionCount = 3;

        private bool CheckForRepetition(SocketUserMessage msg)
        {
            if (lastMessages.Count == 0)
            {
                lastMessages.Add(msg);
                return false;
            }
            else
            {
                var lastMsg = lastMessages[lastMessages.Count - 1];
                if (!(lastMsg.Author.Id != msg.Author.Id && lastMsg.Content.Equals(msg.Content, StringComparison.Ordinal)))
                    lastMessages.Clear();

                lastMessages.Add(msg);
            }

            if (lastMessages.Count >= repetitionCount)
            {
                lastMessages.Clear();
                return true;
            }

            return false;
        }

        #endregion Repetition
    }
}