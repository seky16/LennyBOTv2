using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace LennyBOTv2.Services
{
    public class MessageHandlingService
    {
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _config;
        private readonly ulong _dmLogChannelId;

        public MessageHandlingService(DiscordSocketClient client, IConfiguration config)
        {
            _client = client;
            _config = config;
            _lastMsgId = Convert.ToUInt64(_config["msgCounter:msgId"]);
            _msgCount = Convert.ToUInt64(_config["msgCounter:msgCount"]);
            _msgCounterChannelId = Convert.ToUInt64(_config["msgCounter:channelId"]);
            _dmLogChannelId = Convert.ToUInt64(_config["dmLogChannel"]);
        }

        #region Message counter

        private readonly ulong _msgCounterChannelId;
        private ulong _lastMsgId;
        private ulong _msgCount;

        internal ulong GetMessageCount()
        {
            return _msgCount;
        }

        internal Task MessageDeleted(Cacheable<IMessage, ulong> msg, ISocketMessageChannel channel)
        {
            if (channel.Id == _msgCounterChannelId)
            {
                _msgCount--;
            }

            return Task.CompletedTask;
        }

        internal async Task MessageReceived(SocketMessage rawMessage)
        {
            if (rawMessage.Channel.Id == _msgCounterChannelId)
            {
                var msgs = await rawMessage.Channel.GetMessagesAsync(_lastMsgId, Direction.After, int.MaxValue, CacheMode.AllowDownload).FlattenAsync().ConfigureAwait(false);
                _msgCount += Convert.ToUInt64(msgs.Count());
                _lastMsgId = rawMessage.Id;

                if (_msgCount % 10_000 == 0)
                {
                    await rawMessage.Channel.SendMessageAsync($"🎉 {((SocketTextChannel)rawMessage.Channel).Mention} has {_msgCount:N0} messages 🎉 FeelsBirthdayMan ").ConfigureAwait(false);
                }
            }
        }

        #endregion Message counter

        #region Repetition

        private const int RepetitionCount = 3;
        private readonly List<SocketUserMessage> _lastMessages = new List<SocketUserMessage>();

        internal bool CheckForRepetition(SocketUserMessage msg)
        {
            if (_lastMessages.Count == 0)
            {
                _lastMessages.Add(msg);
                return false;
            }
            else
            {
                var lastMsg = _lastMessages[_lastMessages.Count - 1];
                if (lastMsg.Author.Id == msg.Author.Id || !lastMsg.Content.Equals(msg.Content, StringComparison.Ordinal))
                    _lastMessages.Clear();

                _lastMessages.Add(msg);
            }

            if (_lastMessages.Count >= RepetitionCount)
            {
                _lastMessages.Clear();
                return true;
            }

            return false;
        }

        #endregion Repetition

        internal async Task LogDMMessageAsync(SocketUserMessage message)
        {
            if (_client.GetChannel(_dmLogChannelId) is not ITextChannel channel)
                return;

            await channel.SendMessageAsync(embed: new EmbedBuilder()
                .WithAuthor(message.Author)
                .WithCurrentTimestamp()
                .WithDescription(message.Content)
                .Build()).ConfigureAwait(false);
            foreach (var embed in message.Embeds)
                await channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
            foreach (var attachment in message.Attachments)
                await channel.SendMessageAsync(attachment.Url).ConfigureAwait(false);
        }
    }
}
