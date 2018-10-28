using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
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
                await Task.Delay(seconds * 1000);
                try
                {
                    await msg.DeleteAsync();
                }
                catch (Exception ex)
                {
                    await LoggingService.LogException(ex);
                }
            });
            return msg;
        }

        public static IMessage ModifyAfter(this IUserMessage msg, string message, int seconds)
        {
            Task.Run(async () =>
            {
                await Task.Delay(seconds * 1000);
                try
                {
                    await msg.ModifyAsync(x => x.Content = message);
                }
                catch (Exception ex)
                {
                    await LoggingService.LogException(ex);
                }
            });
            return msg;
        }

        public static async Task<IMessage> GetLastMessageAsync(this ITextChannel channel)
            => (await channel.GetMessagesAsync(1).FlattenAsync()).FirstOrDefault();

        public static string ToPragueTimeString(this DateTime dateTime)
        {
            var utcDateTime = dateTime.ToUniversalTime();
            var instant = Instant.FromDateTimeUtc(utcDateTime);
            var zone = DateTimeZoneProviders.Tzdb["Europe/Prague"];
            var date = new ZonedDateTime(instant, zone);
            return $"{date.Day}.{date.Month}.{date.Year} {date.Hour:D2}:{date.Minute:D2}:{date.Second:D2}";
        }

        public static string ToPragueTimeString(this DateTimeOffset dateTimeOffset)
        {
            var utcDateTime = dateTimeOffset.UtcDateTime;
            var instant = Instant.FromDateTimeUtc(utcDateTime);
            var zone = DateTimeZoneProviders.Tzdb["Europe/Prague"];
            var date = new ZonedDateTime(instant, zone);
            return $"{date.Day}.{date.Month}.{date.Year} {date.Hour:D2}:{date.Minute:D2}:{date.Second:D2}";
        }

        public static string GetNickname(this IUser user)
            => (user as IGuildUser)?.Nickname ?? user.Username;
    }
}