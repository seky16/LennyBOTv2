using System;
using System.Threading.Tasks;
using Discord.Commands;
using LennyBOTv2.Services;

namespace LennyBOTv2.Preconditions
{
    internal class IsBotOwner : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var isOwner = context.User.Id.ToString() == LennyServiceProvider.Instance.Config["owner"];
            return isOwner ?
                Task.FromResult(PreconditionResult.FromSuccess()) :
                Task.FromResult(PreconditionResult.FromError($"{context.User.Username} - not a bot owner."));
        }
    }
}