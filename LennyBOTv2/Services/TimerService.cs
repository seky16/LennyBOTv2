using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace LennyBOTv2.Services
{
    public class TimerService
    {
        private static readonly DateTime _festival = new DateTime(2021, 7, 16, 0, 0, 0, DateTimeKind.Utc);

        private static int _last = 0;

        [SuppressMessage("Code Quality", "IDE0052:Remove unread private members", Justification = "Won't work without it, as Timer gets collected by GC")]
        private readonly Timer _timer;

        public TimerService(DiscordSocketClient client, IConfiguration config)
        {
            _timer = new Timer(async _ =>
            {
                if (client.ConnectionState != ConnectionState.Connected)
                    return;

                if (client.GetChannel(Convert.ToUInt64(config["msgCounter:channelId"])) is SocketTextChannel chan)
                {
                    var eta = (_festival - DateTime.UtcNow.AddHours(1).Date).Days;
                    if (eta == _last)
                        return;

                    _last = eta;
                    await chan.ModifyAsync(ch => ch.Topic = $"🌌 Liquicity Festival 🎶 T-Minus {eta} days 🚀").ConfigureAwait(false);
                }
            },
            null,
            TimeSpan.Zero,
            TimeSpan.FromMinutes(1));
        }
    }
}
