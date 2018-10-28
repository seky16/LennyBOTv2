using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private DiscordSocketClient _client;
        private readonly CommandService _commands = new CommandService();
        private IServiceProvider _services;
        private readonly IConfiguration _config;

        public Program()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                MessageCacheSize = 1000
            });
            _config = BuildConfig();
        }

        private IConfiguration BuildConfig()
        {
            var cwd = Directory.GetCurrentDirectory();
            var builder = new ConfigurationBuilder();
            var test = Directory.GetFiles(cwd).Any(f => f.ToLowerInvariant().Contains("debug"));
            var file = test ? "Files/testConfig.json" : "Files/config.json";
            LoggingService.LogInfo($"{file} loaded", "Config");
            return builder.SetBasePath(cwd).AddJsonFile(file).Build();
        }

        private static void Main(string[] args)
        => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            _client.Log += LoggingService.LogAsync;
            _commands.Log += LoggingService.LogAsync;

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_config)
                .AddSingleton(_commands)
                .AddSingleton<LoggingService>()

                .BuildServiceProvider();
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            _client.MessageReceived += MessageReceived;

            await _client.LoginAsync(TokenType.Bot, _config["token"]);
            await _client.StartAsync();

            await Task.Delay(-1);
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

            if (result.Error.HasValue &&
                result.Error.Value != CommandError.UnknownCommand)
                await context.Channel.SendMessageAsync(result.ToString());
            else if (result.Error.HasValue &&
                result.Error.Value == CommandError.UnknownCommand)
                await context.Message.AddReactionAsync(new Emoji("❓"));
        }
    }
}