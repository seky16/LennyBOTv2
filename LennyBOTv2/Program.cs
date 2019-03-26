using System;
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
        public static bool IsDebug = Directory.GetCurrentDirectory().ToLowerInvariant().Contains("debug");
        private readonly CommandService _commands = new CommandService();
        private readonly IConfiguration _config;
        private DiscordSocketClient _client;
        private IServiceProvider _services;

        public Program()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 1000
            });
            _config = BuildConfig();
        }

        public async Task MainAsync()
        {
            _client.Log += LoggingService.LogAsync;
            _commands.Log += LoggingService.LogAsync;

            await _client.LoginAsync(TokenType.Bot, _config["token"]);
            await _client.StartAsync();

            _services = LennyServiceProvider.Instance.Build(_client, _config, _commands);

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            _client.MessageReceived += MessageReceived;

            await Task.Delay(-1);
        }

        private static void Main(string[] args)
        => new Program().MainAsync().GetAwaiter().GetResult();

        private IConfiguration BuildConfig()
        {
            var cwd = Directory.GetCurrentDirectory();
            var file = IsDebug ? "Files/testConfig.json" : "Files/config.json";
            LoggingService.LogInfo($"{file} loaded", "Config");
            return new ConfigurationBuilder().SetBasePath(cwd).AddJsonFile(file).Build();
        }

        private async Task MessageReceived(SocketMessage rawMessage)
        {
            // Ignore system messages and messages from bots
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            int argPos = 0;
            if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasStringPrefix(_config["prefix"], ref argPos))) return;

            var context = new SocketCommandContext(_client, message);
            var result = await _commands.ExecuteAsync(context, argPos, _services);

            if (result.Error.HasValue)
            {
                switch (result.Error.Value)
                {
                    case CommandError.UnknownCommand:
                        await context.Message.AddReactionAsync(new Emoji("❓"));
                        break;

                    case CommandError.Exception:
                        //await LoggingService.LogException(((ExecuteResult)result).Exception);
                        break;

                    case CommandError.ParseFailed:
                    case CommandError.BadArgCount:
                    case CommandError.ObjectNotFound:
                    case CommandError.MultipleMatches:
                    case CommandError.UnmetPrecondition:
                    case CommandError.Unsuccessful:
                    default:
                        await LoggingService.LogError($"{message.Author} '{message.Content}'", result.ErrorReason);
                        await message.AddReactionAsync(new Emoji("⚠"));
                        break;
                }
            }
        }
    }
}