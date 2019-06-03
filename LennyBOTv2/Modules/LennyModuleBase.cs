using System;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using LennyBOTv2.Services;
using Microsoft.Extensions.Configuration;

namespace LennyBOTv2.Modules
{
    public class LennyModuleBase : InteractiveBase<SocketCommandContext>
    {
        protected IConfiguration Config = (IConfiguration)LennyServiceProvider.Instance.ServiceProvider.GetService(typeof(IConfiguration));

        public SocketTextChannel GetNotificationChannel()
        {
            var channelId = Convert.ToUInt64(Config["notificationChannel"]);
            return this.Context.Client.GetChannel(channelId) as SocketTextChannel;
        }

        public SocketUser GetOwner()
        {
            var ownerId = Convert.ToUInt64(Config["owner"]);
            return this.Context.Client.GetUser(ownerId);
        }

        public async Task MarkCmdFailedAsync(string reason = "")
        {
            await this.Context.Message.AddReactionAsync(new Emoji("⚠")).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(reason))
                reason = $" (Reason: {reason})";
            var msg = $"{this.Context.User.Username} '{this.Context.Message.Content}' failed. {reason.Trim()}";
            await LoggingService.LogError(msg).ConfigureAwait(false);
            await this.GetNotificationChannel().SendMessageAsync($"{this.GetOwner().Mention}\n{msg}").ConfigureAwait(false);
        }
    }
}