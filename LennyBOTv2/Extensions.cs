using System;
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

        public static async Task<IMessage?> GetLastMessageAsync(this ITextChannel channel)
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

        public static async Task MarkCmdOkAsync(this SocketCommandContext context) => await context.Message.AddReactionAsync(new Emoji("✅")).ConfigureAwait(false);

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
            var date = dateTime.ToUniversalTime().UtcToPragueZonedDateTime();
            return $"{date.Day:D2}.{date.Month:D2}.{date.Year:D4} {date.Hour:D2}:{date.Minute:D2}:{date.Second:D2}";
        }

        // todo: rewrite?
        public static string ToPragueTimeString(this DateTimeOffset dateTimeOffset)
        {
            var date = dateTimeOffset.UtcDateTime.UtcToPragueZonedDateTime();
            return $"{date.Day:D2}.{date.Month:D2}.{date.Year:D4} {date.Hour:D2}:{date.Minute:D2}:{date.Second:D2}";
        }

        public static ZonedDateTime UtcToPragueZonedDateTime(this DateTime utcDateTime)
        {
            var instant = Instant.FromDateTimeUtc(utcDateTime);
            var zone = DateTimeZoneProviders.Tzdb["Europe/Prague"];
            return new ZonedDateTime(instant, zone);
        }

        public static bool AnyInnerException<T>(this Exception? ex) where T : Exception
        {
            while (ex is not null)
            {
                if (ex is T)
                    return true;

                ex = ex.InnerException;
            }

            return false;
        }
        public static string Truncate(this string str, int size, string appendix = "...")
        {
            if (str is null)
                throw new ArgumentNullException(nameof(str));

            if (appendix is null)
                appendix = string.Empty;

            if (str.Length <= size)
                return str;
            else
                return str.Substring(0, size - appendix.Length) + appendix;
        }


    }
}
