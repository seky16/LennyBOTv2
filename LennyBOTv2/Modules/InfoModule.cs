using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace LennyBOTv2.Modules
{
    public class InfoModule : LennyModuleBase
    {
        [Command("info")]
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
        }

        [Command("avatar")]
        public Task UserAvatarAsync(SocketUser user = null)
        {
            user = user ?? this.Context.User;
            var avatar = user.GetAvatarUrl(size: 2048) ?? "This user has no avatar";
            return this.ReplyAsync(avatar);
        }

        private static string GetUptime() => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");

        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();
    }
}