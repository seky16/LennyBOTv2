using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using NodaTime;

namespace LennyBOTv2.Services
{
    public class TimerService
    {
        private readonly DiscordSocketClient _client;

        private readonly IConfiguration _config;

        [SuppressMessage("Code Quality", "IDE0052:Remove unread private members", Justification = "Won't work without it, as Timer gets collected by GC")]
        private readonly Timer _timer;

        public TimerService(DiscordSocketClient client, IConfiguration config)
        {
            _client = client;
            _config = config;
            _timer = new Timer(TimerCallback, DateTime.UtcNow, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        private async void TimerCallback(object? state)
        {
            await LoggingService.LogDebugAsync($"{nameof(TimerCallback)} started", nameof(TimerService)).ConfigureAwait(false);
            if (_client.ConnectionState != ConnectionState.Connected)
                return;

            if (state is not DateTime utcNow)
                return;

            await UpdateChannelTopic(utcNow).ConfigureAwait(false);
            await SendFrogMsg(utcNow).ConfigureAwait(false);
        }

        #region Channel topic

        private async Task UpdateChannelTopic(DateTime utcNow)
        {
            try
            {
                await LoggingService.LogDebugAsync($"{nameof(UpdateChannelTopic)} started", nameof(TimerService)).ConfigureAwait(false);
                if (_client.GetChannel(Convert.ToUInt64(_config["channelTopic:channelId"])) is not SocketTextChannel chan)
                    return;

                if (!DateTime.TryParse(_config["channelTopic:date"], out var date))
                    return;

                var localDate = LocalDate.FromDateTime(date);

                var eta = Period.Between(localDate, utcNow.UtcToPragueZonedDateTime().Date, PeriodUnits.Days).Days;
                if (eta == CacheService.TimerService_ChannelTopicEta)
                    return;

                CacheService.TimerService_ChannelTopicEta = eta;

                var text = _config["channelTopic:text"];

                if (string.IsNullOrEmpty(text))
                    return;

                text = Helpers.ReplaceEmotesInText(text).Replace("{eta}", (eta > 0 ? "+" : "") + eta);

                await LoggingService.LogInfoAsync($"Updating {chan} topic to '{text}'", nameof(TimerService)).ConfigureAwait(false);
                await chan.ModifyAsync(ch => ch.Topic = text).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await LoggingService.LogExceptionAsync(ex, nameof(TimerService), $"{nameof(UpdateChannelTopic)} failed").ConfigureAwait(false);
            }
        }

        #endregion Channel topic

        #region Frog msg

        private static Stream GetFrogImage(ZonedDateTime zonedDateTime)
        {
            var img = System.Drawing.Image.FromFile("Files/frog.jpg");
            var gr = Graphics.FromImage(img);
            var font = new Font("Times New Roman", 20, FontStyle.Regular);
            var strFormat = new StringFormat() { Alignment = StringAlignment.Center };
            gr.DrawString("Gentlemen, it is with great pleasure to inform you that",
                font,
                Brushes.White,
                new RectangleF(20, 20, img.Width - 40, img.Height - 20),
                strFormat);
            gr.DrawString($"today is {zonedDateTime.ToString("dddd, MMMM d", null)}",
                font,
                Brushes.White,
                new RectangleF(20, 450, img.Width - 40, img.Height - 450),
                strFormat);
            var stream = new MemoryStream();
            img.Save(stream, img.RawFormat);
            stream.Position = 0;
            return stream;
        }

        private async Task SendFrogMsg(DateTime utcNow)
        {
            try
            {
                await LoggingService.LogDebugAsync($"{nameof(SendFrogMsg)} started", nameof(TimerService)).ConfigureAwait(false);
                var zonedDateTime = utcNow.UtcToPragueZonedDateTime();

                if (!TimeSpan.TryParse(_config["frogMsg:time"], out var time) || CacheService.TimerService_LastSentFrogMsg >= zonedDateTime.Minus(Duration.FromTimeSpan(time)).Date)
                    return;

                if (_client.GetUser(Convert.ToUInt64(_config["frogMsg:userId"])) is not SocketUser user)
                    return;

                var filename = zonedDateTime.ToString("yyyyMMddhhmm", null) + ".jpg";

                await LoggingService.LogInfoAsync($"Sending frog msg to {user.GetNickname()}", nameof(TimerService)).ConfigureAwait(false);
                await user.SendFileAsync(GetFrogImage(zonedDateTime), filename, embed: new EmbedBuilder().WithImageUrl($"attachment://{filename}").Build()).ConfigureAwait(false);
                CacheService.TimerService_LastSentFrogMsg = zonedDateTime.Date;
            }
            catch (Exception ex)
            {
                await LoggingService.LogExceptionAsync(ex, nameof(TimerService), $"{nameof(SendFrogMsg)} failed").ConfigureAwait(false);
            }
        }

        #endregion Frog msg
    }
}
