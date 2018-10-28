using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using LennyBOTv2.Preconditions;
using Microsoft.Extensions.Configuration;

namespace LennyBOTv2.Modules
{
    public class BotOwnerModule : LennyModuleBase
    {
        private readonly IConfiguration _config;

        public BotOwnerModule(IConfiguration config)
        {
            _config = config;
        }

        [Command("say"), Alias("s")]
        [IsBotOwner]
        public Task SayCmdAsync([Remainder]string text)
            => this.ReplyAsync(text);

        [Command("exit", RunMode = RunMode.Async)]
        [IsBotOwner]
        public async Task ExitCmdAsync()
        {
            await this.ReplyAsync("Shutting down... :zzz:");
            await this.Context.Client.SetStatusAsync(UserStatus.Invisible);
            await this.Context.Client.StopAsync();
            Environment.Exit(0);
        }

        [Command("restart", RunMode = RunMode.Async)]
        [IsBotOwner]
        public async Task RestartCmdAsync()
        {
            var msg = await this.ReplyAsync("Restarting... :arrows_counterclockwise:");
            await this.Context.Client.StopAsync();
            await this.Context.Client.LogoutAsync();
            await this.Context.Client.LoginAsync(TokenType.Bot, _config["token"]);
            await this.Context.Client.StartAsync();
            await msg.ModifyAsync(m => m.Content = "Restarted :white_check_mark:");
        }

        [Command("botnick")]
        [IsBotOwner]
        public async Task BotNickCmdAsync([Remainder]string name)
            => await Context?.Guild?.CurrentUser?.ModifyAsync(x => x.Nickname = name);

        [Command("playing")]
        [IsBotOwner]
        public Task PlayingCmdAsync([Remainder]string game)
            => Context?.Client?.SetGameAsync(game);
    }
}