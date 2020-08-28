using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace LennyBOTv2.Services
{
    internal static class MsgCounterService
    {
        private static ulong _lastMsgId = 748876264714010644;

        public static int MsgCount { get; private set; } = 564_694;

        public static async Task UpdateMsgCountAsync(SocketMessage rawMessage)
        {
            var msgs = await rawMessage.Channel.GetMessagesAsync(_lastMsgId, Direction.After, int.MaxValue, CacheMode.AllowDownload).FlattenAsync().ConfigureAwait(false);
            MsgCount += msgs.Count();
            _lastMsgId = rawMessage.Id;
        }
    }
}
