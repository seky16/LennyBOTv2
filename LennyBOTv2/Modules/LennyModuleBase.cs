﻿using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using LennyBOTv2.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LennyBOTv2.Modules
{
    public class LennyModuleBase : InteractiveBase<SocketCommandContext>
    {
        public async Task MarkCmdFailedAsync(string reason = "")
        {
            await this.Context.Message.AddReactionAsync(new Emoji("⚠"));
            if (!string.IsNullOrWhiteSpace(reason))
                reason = $" (Reason: {reason})";
            await LoggingService.LogError($"{this.Context.User.Username} '{this.Context.Message.Content}' failed. {reason.Trim()}");
        }
    }
}