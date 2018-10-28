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
            var config = services.GetRequiredService(typeof(IConfiguration)) as IConfiguration;
            if (config is null)
            {
                return Task.FromResult(PreconditionResult.FromError("Cannot load config."));
            }

            var isOwner = context.User.Id.ToString() == config["owner"];
            return isOwner ?
                Task.FromResult(PreconditionResult.FromSuccess()) :
                Task.FromResult(PreconditionResult.FromError("You are not bot owner."));
        }
    }
}