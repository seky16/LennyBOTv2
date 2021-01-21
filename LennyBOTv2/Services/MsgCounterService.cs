using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace LennyBOTv2.Services
{
    internal static class MsgCounterService
    {
        private static ulong _lastMsgId = 801608614518194196;

        internal static int MsgCount { get; private set; } = 644_759;

        internal static void DecreaseCount()
        {
            MsgCount--;
        }

        internal static async Task UpdateMsgCountAsync(SocketMessage rawMessage)
        {
            var msgs = await rawMessage.Channel.GetMessagesAsync(_lastMsgId, Direction.After, int.MaxValue, CacheMode.AllowDownload).FlattenAsync().ConfigureAwait(false);
            MsgCount += msgs.Count();
            _lastMsgId = rawMessage.Id;
        }
    }
}
