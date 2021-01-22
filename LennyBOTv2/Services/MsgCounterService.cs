using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace LennyBOTv2.Services
{
    internal static class MsgCounterService
    {
        private static readonly IConfiguration _config = LennyServiceProvider.Instance.Config;
        private static ulong _lastMsgId = Convert.ToUInt64(_config["msgCounter:msgId"]);
        internal static ulong MsgCount { get; private set; } = Convert.ToUInt64(_config["msgCounter:msgCount"]);

        internal static void DecreaseCount()
        {
            MsgCount--;
        }

        internal static async Task UpdateMsgCountAsync(SocketMessage rawMessage)
        {
            var msgs = await rawMessage.Channel.GetMessagesAsync(_lastMsgId, Direction.After, int.MaxValue, CacheMode.AllowDownload).FlattenAsync().ConfigureAwait(false);
            MsgCount += Convert.ToUInt64(msgs.Count());
            _lastMsgId = rawMessage.Id;
        }
    }
}
