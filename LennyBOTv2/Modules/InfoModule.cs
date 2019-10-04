using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LennyBOTv2.Services;

namespace LennyBOTv2.Modules
{
    public class InfoModule : LennyModuleBase
    {
        public InfoModule(CommandService commandService)
        {
            CommandService = commandService;
            Watch = Stopwatch.StartNew();
        }

        public CommandService CommandService { get; }
        public Stopwatch Watch { get; }

        /*[Command("info")]
[Alias("about", "stats")]
public async Task InfoCmdAsync()
{
var app = await this.Context.Client.GetApplicationInfoAsync();

await this.ReplyAsync(
$"{Format.Bold("Info")}\n"
+ $"- Author: {app.Owner} ({app.Owner.Id})\n"
+ $"- Library: Discord.Net ({DiscordConfig.Version})\n"
+ $"- Runtime: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.ProcessArchitecture} "
+ $"({RuntimeInformation.OSDescription} {RuntimeInformation.OSArchitecture})\n"
+ $"- Uptime: {GetUptime()}\n\n"

+ $"{Format.Bold("Stats")}\n"
+ $"- Heap Size: {GetHeapSize()}MiB\n"
+ $"- Guilds: {this.Context.Client.Guilds.Count}\n"
+ $"- Channels: {this.Context.Client.Guilds.Sum(g => g.Channels.Count)}\n"
+ $"- Users: {this.Context.Client.Guilds.Sum(g => g.Users.Count)}\n")
;
}*/

        [Command("help")]
        public async Task HelpCmdAsync()
        {
            var embed = new EmbedBuilder().WithTitle(":keyboard: Available commands").WithColor(new Color(255, 255, 255));
            var prefix = Config["prefix"];
            foreach (var module in CommandService.Modules)
            {
                var sb = new StringBuilder();
                foreach (var cmd in module.Commands)
                {
                    var result = await cmd.CheckPreconditionsAsync(Context, LennyServiceProvider.Instance.ServiceProvider).ConfigureAwait(false);
                    if (result.IsSuccess)
                        sb.Append(prefix).AppendLine(cmd.Name);
                }
                //todo: show botowner only in seky16 guild
                if (!string.IsNullOrEmpty(sb.ToString()) && module.Name != "BotOwnerModule")
                    embed.AddField(module.Name.Replace("Module", ""), sb.ToString());
            }
            await ReplyEmbedAsync(embed).ConfigureAwait(false);
        }

        [Command("ping")]
        public async Task PingCmdAsync()
        {
            Watch.Stop();
            var execution = Watch.ElapsedMilliseconds;
            var ping = Context.Client.Latency;
            var embed = new EmbedBuilder()
                .WithTitle(":ping_pong: Pong!")
                .WithDescription($"Ping: {ping} ms\nExecution: {execution} ms")
                .WithFooter($"Uptime: {GetUptime()}");
            await ReplyEmbedAsync(embed).ConfigureAwait(false);
        }

        [Command("avatar")]
        public async Task UserAvatarAsync(SocketUser? user = null)
        {
            user ??= Context.User;
            var avatar = user.GetAvatarUrl(size: 2048) ?? "This user has no avatar";
            await ReplyAsync(avatar).ConfigureAwait(false);
        }

        //private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();

        private static string GetUptime() => (System.DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
    }
}
