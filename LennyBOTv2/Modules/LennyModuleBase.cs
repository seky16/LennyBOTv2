using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using LennyBOTv2.Services;
using Microsoft.Extensions.Configuration;

namespace LennyBOTv2.Modules
{
    public class LennyModuleBase : InteractiveBase<SocketCommandContext>
    {
        protected static IConfiguration Config => LennyServiceProvider.Instance.Config;

        public Task<IUserMessage> ReplyEmbedAsync(Embed embed)
            => ReplyAsync("", false, embed, null);

        public Task<IUserMessage> ReplyEmbedAsync(EmbedBuilder builder)
            => ReplyAsync("", false, builder.Build(), null);
    }
}
