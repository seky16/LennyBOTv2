using System;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using LiteDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OMDbApiNet;

namespace LennyBOTv2.Services
{
    internal sealed class LennyServiceProvider
    {
        private static readonly Lazy<LennyServiceProvider> _lazy = new Lazy<LennyServiceProvider>(() => new LennyServiceProvider());

        public static LennyServiceProvider Instance => _lazy.Value;

        public IConfiguration Config
        {
            get
            {
                if (!(ServiceProvider?.GetService(typeof(IConfiguration)) is IConfiguration config))
                    throw new NullReferenceException("Configuration didn't load properly.");

                return config;
            }
        }

        public IServiceProvider? ServiceProvider { get; private set; }

        public static LiteDatabase OpenDB()
        {
            var db = new LiteDatabase("Files/Lenny.db");
            db.UtcDate = true;
            db.Mapper.EnumAsInteger = true;
            return db;
        }

        public IServiceProvider Build(DiscordSocketClient client, IConfiguration config, CommandService commands)
        {
            if (ServiceProvider is not null)
                return ServiceProvider;

            ServiceProvider = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(config)
                .AddSingleton(commands)
                .AddSingleton(new InteractiveService((BaseSocketClient)client))
                .AddSingleton(new ReliabilityService(client))
                .AddSingleton(new AsyncOmdbClient(config["omdbAPIkey"], true))
                .AddSingleton(new YouTubeService(new BaseClientService.Initializer() { ApiKey = config["youtubeAPIkey"], ApplicationName = "LennyBOT" }))
                .AddSingleton(new TimerService(client, config))
                .AddSingleton(new MessageHandlingService(client, config))
                .AddSingleton<AmongUsService>()
                .AddSingleton<WeatherService>()
                .AddSingleton<Random>()

                .BuildServiceProvider();

            // disable buttons in InteractiveService
            PaginatedAppearanceOptions.Default.DisplayInformationIcon = false;
            PaginatedAppearanceOptions.Default.JumpDisplayOptions = JumpDisplayOptions.Never;
            PaginatedAppearanceOptions.Default.Stop = null;

            // third-party
            FixerSharp.Fixer.SetApiKey(config["fixerAPIkey"]);

            return ServiceProvider;
        }
    }
}
