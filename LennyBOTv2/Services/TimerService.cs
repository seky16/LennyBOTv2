﻿using System;
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
        [SuppressMessage("Code Quality", "IDE0052:Remove unread private members", Justification = "Won't work without it, as Timer gets collected by GC")]
        private readonly Timer _timer;

        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _config;

        public TimerService(DiscordSocketClient client, IConfiguration config)
        {
            _client = client;
            _config = config;
            _timer = new Timer(TimerCallback, DateTime.UtcNow, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        private async void TimerCallback(object? state)
        {
            if (_client.ConnectionState != ConnectionState.Connected)
                return;

            if (state is not DateTime utcNow)
                return;

            await UpdateFestivalEta(utcNow).ConfigureAwait(false);
            await SendFrogMsg(utcNow).ConfigureAwait(false);
        }

        #region Festival ETA

        private static readonly DateTime _festivalDate = new DateTime(2021, 7, 16, 0, 0, 0, DateTimeKind.Utc);

        private static int _lastEta = 0;

        private async Task UpdateFestivalEta(DateTime utcNow)
        {
            if (_client.GetChannel(Convert.ToUInt64(_config["msgCounter:channelId"])) is not SocketTextChannel chan)
                return;

            var eta = (_festivalDate - utcNow.AddHours(1).Date).Days;
            if (eta == _lastEta)
                return;

            _lastEta = eta;
            await chan.ModifyAsync(ch => ch.Topic = $"🌌 Liquicity Festival 🎶 T-Minus {eta} days 🚀").ConfigureAwait(false);
        }

        #endregion Festival ETA

        #region Frog msg

        private static LocalDate _lastSend = DateTime.UtcNow.UtcToPragueZonedDateTime().Date.PlusDays(-1);

        private async Task SendFrogMsg(DateTime utcNow)
        {
            var zonedDateTime = utcNow.UtcToPragueZonedDateTime();
            if (!TimeSpan.TryParse(_config["frogMsg:time"], out var time) || _lastSend >= zonedDateTime.Minus(Duration.FromTimeSpan(time)).Date)
                return;

            if (_client.GetUser(Convert.ToUInt64(_config["frogMsg:userId"])) is not SocketUser user)
                return;

            var filename = zonedDateTime.ToString("yyyyMMddhhmm", null) + ".jpg";

            await user.SendFileAsync(GetFrogImage(zonedDateTime), filename, embed: new EmbedBuilder().WithImageUrl($"attachment://{filename}").Build()).ConfigureAwait(false);
            _lastSend = zonedDateTime.Date;
        }

        private Stream GetFrogImage(ZonedDateTime zonedDateTime)
        {
            var img = System.Drawing.Image.FromFile("Files/frog.jpg");
            var gr = Graphics.FromImage(img);
            var font = new Font("Times New Roman", 34, FontStyle.Regular);
            var strFormat = new StringFormat() { Alignment = StringAlignment.Center };
            gr.DrawString("Gentlemen, it is with great pleasure to inform you that",
                font,
                Brushes.White,
                new RectangleF(20, 20, img.Width - 40, img.Height - 20),
                strFormat);
            gr.DrawString($"today is {zonedDateTime.DayOfWeek}",
                font,
                Brushes.White,
                new RectangleF(20, 450, img.Width - 40, img.Height - 450),
                strFormat);
            var stream = new MemoryStream();
            img.Save(stream, img.RawFormat);
            stream.Position = 0;
            return stream;
        }

        #endregion Frog msg
    }
}
