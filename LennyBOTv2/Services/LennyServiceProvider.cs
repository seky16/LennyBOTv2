using System;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OMDbApiNet;

namespace LennyBOTv2.Services
{
    internal sealed class LennyServiceProvider
    {
        private static readonly Lazy<LennyServiceProvider> _lazy = new Lazy<LennyServiceProvider>(() => new LennyServiceProvider());

        public static LennyServiceProvider Instance => _lazy.Value;

        public IServiceProvider ServiceProvider { get; private set; } = null;

        public IServiceProvider Build(DiscordSocketClient client, IConfiguration config, CommandService commands)
        {
            if (!(ServiceProvider is null))
                return ServiceProvider;

            ServiceProvider = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(config)
                .AddSingleton(commands)
                .AddSingleton(new InteractiveService((BaseSocketClient)client))
                .AddSingleton(new AsyncOmdbClient(config["omdbAPIkey"], true))
                .AddSingleton(new YouTubeService(new BaseClientService.Initializer() { ApiKey = config["youtubeAPIkey"], ApplicationName = "LennyBOT" }))

                .BuildServiceProvider();

            // disable buttons in InteractiveService
            PaginatedAppearanceOptions.Default.DisplayInformationIcon = false;
            PaginatedAppearanceOptions.Default.JumpDisplayOptions = JumpDisplayOptions.Never;
            PaginatedAppearanceOptions.Default.Stop = null;

            new ReliabilityService(client);

            // third-party
            FixerSharp.Fixer.SetApiKey(config["fixerAPIkey"]);

            return ServiceProvider;
        }
    }
}