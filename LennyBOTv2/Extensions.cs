﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LennyBOTv2.Services;
using NodaTime;

namespace LennyBOTv2
{
    public static class Extensions
    {
        public static IMessage DeleteAfter(this IUserMessage msg, int seconds)
        {
            Task.Run(async () =>
            {
                await Task.Delay(seconds * 1000).ConfigureAwait(false);
                await msg.DeleteAsync().ConfigureAwait(false);
            });
            return msg;
        }

        public static async Task<IMessage> GetLastMessageAsync(this ITextChannel channel)
            => (await channel.GetMessagesAsync(1).FlattenAsync().ConfigureAwait(false)).FirstOrDefault();

        public static string GetNickname(this IUser user)
            => (user as IGuildUser)?.Nickname ?? user.Username ?? "";

        public static SocketTextChannel GetNotificationChannel(this SocketCommandContext context)
        {
            var channelId = Convert.ToUInt64(LennyServiceProvider.Instance.Config["notificationChannel"]);
            return (SocketTextChannel)context.Client.GetChannel(channelId);
        }

        public static SocketUser GetOwner(this SocketCommandContext context)
        {
            var ownerId = Convert.ToUInt64(LennyServiceProvider.Instance.Config["owner"]);
            return context.Client.GetUser(ownerId);
        }

        public static bool HasRole(this IUser user, ulong roleId)
                            => (user as IGuildUser)?.RoleIds.Contains(roleId) ?? false;

        public static async Task MarkCmdFailedAsync(this SocketCommandContext context, string reason = "")
        {
            await context.Message.AddReactionAsync(new Emoji("⚠")).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(reason))
                reason = $"(Reason: {reason})";
            var msg = $"{context.Guild}/{context.Channel}/{context.User.Username} '{context.Message.Content}' failed. {reason}";
            await LoggingService.LogErrorAsync(msg, "Command").ConfigureAwait(false);

#if !DEBUG
            await context.GetNotificationChannel().SendMessageAsync($"{context.GetOwner().Mention}\n{msg}").ConfigureAwait(false);
#endif
        }

        public static IMessage ModifyAfter(this IUserMessage msg, int seconds, string newContent)
        {
            Task.Run(async () =>
            {
                await Task.Delay(seconds * 1000).ConfigureAwait(false);
                await msg.ModifyAsync(x => x.Content = newContent).ConfigureAwait(false);
            });
            return msg;
        }

        // todo: rewrite?
        public static string ToPragueTimeString(this DateTime dateTime)
        {
            var utcDateTime = dateTime.ToUniversalTime();
            var instant = Instant.FromDateTimeUtc(utcDateTime);
            var zone = DateTimeZoneProviders.Tzdb["Europe/Prague"];
            var date = new ZonedDateTime(instant, zone);
            return $"{date.Day:D2}.{date.Month:D2}.{date.Year:D4} {date.Hour:D2}:{date.Minute:D2}:{date.Second:D2}";
        }

        // todo: rewrite?
        public static string ToPragueTimeString(this DateTimeOffset dateTimeOffset)
        {
            var utcDateTime = dateTimeOffset.UtcDateTime;
            var instant = Instant.FromDateTimeUtc(utcDateTime);
            var zone = DateTimeZoneProviders.Tzdb["Europe/Prague"];
            var date = new ZonedDateTime(instant, zone);
            return $"{date.Day:D2}.{date.Month:D2}.{date.Year:D4} {date.Hour:D2}:{date.Minute:D2}:{date.Second:D2}";
        }
    }
}
