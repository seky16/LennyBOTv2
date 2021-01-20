using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Discord.Addons.Interactive
{
    public interface IReactionCallback
    {
        SocketCommandContext Context { get; }
        ICriterion<SocketReaction> Criterion { get; }
        RunMode RunMode { get; }
        TimeSpan? Timeout { get; }

        Task<bool> HandleCallbackAsync(SocketReaction reaction);
    }
}