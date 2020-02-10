using System;
using System.Threading;
using Discord;
using Discord.WebSocket;

namespace LennyBOTv2.Services
{
    public class TimerService
    {
        private static readonly DateTime _festival = new DateTime(2020, 7, 18, 0, 0, 0, DateTimeKind.Utc);

        private static int _last = 0;

        public TimerService(DiscordSocketClient client)
        {
            _ = new Timer(async _ =>
            {
                if (client.ConnectionState != ConnectionState.Connected)
                    return;

                var eta = (_festival - DateTime.UtcNow.AddHours(1).Date).Days;
                if (eta == _last)
                    return;

                _last = eta;

                if (client.GetChannel(239504532734869505) is SocketTextChannel chan)
                {
                    await chan.ModifyAsync(ch => ch.Topic = $"🌌 Liquicity Festival 🎶 T-Minus {eta} days 🚀").ConfigureAwait(false);
                }
            },
            null,
            TimeSpan.Zero,
            TimeSpan.FromMinutes(1));
        }
    }
}
