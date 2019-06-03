using System;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LennyBOTv2.Preconditions
{
    internal class IsBotOwner : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (!(services.GetRequiredService(typeof(IConfiguration)) is IConfiguration config))
            {
                return Task.FromResult(PreconditionResult.FromError("Cannot load config."));
            }

            var isOwner = context.User.Id.ToString() == config["owner"];
            return isOwner ?
                Task.FromResult(PreconditionResult.FromSuccess()) :
                Task.FromResult(PreconditionResult.FromError($"{context.User.Username} - not a bot owner."));
        }
    }
}