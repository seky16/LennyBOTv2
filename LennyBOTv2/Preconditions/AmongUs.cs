using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace LennyBOTv2.Preconditions
{
    internal sealed class AmongUsServer : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            return context.Guild.Id == 750776488365654137 ?
                Task.FromResult(PreconditionResult.FromSuccess()) :
                Task.FromResult(PreconditionResult.FromError("Wrong server"));
        }
    }

    internal sealed class AmongUsWriteStats : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            return context.Guild.Id == 750776488365654137 && context.User.HasRole(751442179670540288) ?
                Task.FromResult(PreconditionResult.FromSuccess()) :
                Task.FromResult(PreconditionResult.FromError("No permission to write stats"));
        }
    }
}
