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
        private IServiceProvider? _services;

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
            _client.MessageReceived += MessageReceived;
            _client.MessageDeleted += MessageDeleted;

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

        private Task MessageDeleted(Cacheable<IMessage, ulong> msg, ISocketMessageChannel channel)
        {
            if (channel.Id == Convert.ToUInt64(_config["msgCounter:channelId"]))
            {
                MsgCounterService.DecreaseCount();
            }

            return Task.CompletedTask;
        }

        private async Task MessageReceived(SocketMessage rawMessage)
        {
            if (rawMessage.Channel.Id == Convert.ToUInt64(_config["msgCounter:channelId"]))
            {
                await MsgCounterService.UpdateMsgCountAsync(rawMessage).ConfigureAwait(false);

                if (MsgCounterService.MsgCount % 10_000 == 0)
                {
                    await rawMessage.Channel.SendMessageAsync($"🎉 {((SocketTextChannel)rawMessage.Channel).Mention} has {MsgCounterService.MsgCount:N0} messages 🎉 FeelsBirthdayMan ").ConfigureAwait(false);
                }
            }

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
            var argPos = 0;
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

        private const int RepetitionCount = 3;
        private readonly List<SocketUserMessage> _lastMessages = new List<SocketUserMessage>();

        private bool CheckForRepetition(SocketUserMessage msg)
        {
            if (_lastMessages.Count == 0)
            {
                _lastMessages.Add(msg);
                return false;
            }
            else
            {
                var lastMsg = _lastMessages[_lastMessages.Count - 1];
                if (!(lastMsg.Author.Id != msg.Author.Id && lastMsg.Content.Equals(msg.Content, StringComparison.Ordinal)))
                    _lastMessages.Clear();

                _lastMessages.Add(msg);
            }

            if (_lastMessages.Count >= RepetitionCount)
            {
                _lastMessages.Clear();
                return true;
            }

            return false;
        }

        #endregion Repetition
    }
}
