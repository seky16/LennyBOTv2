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
            return Context.Client.GetChannel(channelId) as SocketTextChannel;
        }

        public SocketUser GetOwner()
        {
            var ownerId = Convert.ToUInt64(Config["owner"]);
            return Context.Client.GetUser(ownerId);
        }

        public async Task MarkCmdFailedAsync(string reason = "")
        {
            await Context.Message.AddReactionAsync(new Emoji("⚠")).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(reason))
                reason = $" (Reason: {reason})";
            var msg = $"{Context.User.Username} '{Context.Message.Content}' failed. {reason.Trim()}";
            await LoggingService.LogErrorAsync(msg).ConfigureAwait(false);
            await GetNotificationChannel().SendMessageAsync($"{GetOwner().Mention}\n{msg}").ConfigureAwait(false);
        }

        public Task<IUserMessage> ReplyEmbedAsync(Embed embed)
            => ReplyAsync("", false, embed, null);

        public Task<IUserMessage> ReplyEmbedAsync(EmbedBuilder builder)
            => ReplyAsync("", false, builder.Build(), null);
    }
}