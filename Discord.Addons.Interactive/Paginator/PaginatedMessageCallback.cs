using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Discord.Addons.Interactive
{
    public class PaginatedMessageCallback : IReactionCallback
    {
        private readonly PaginatedMessage _pager;
        private readonly int _pages;
        private int _page = 1;

        public PaginatedMessageCallback(InteractiveService interactive,
            SocketCommandContext sourceContext,
            PaginatedMessage pager,
            ICriterion<SocketReaction> criterion = null)
        {
            Interactive = interactive;
            Context = sourceContext;
            Criterion = criterion ?? new EmptyCriterion<SocketReaction>();
            _pager = pager;
            _pages = _pager.Pages.Count();
            if (_pager.Pages is IEnumerable<EmbedFieldBuilder>)
            {
                _pages = ((_pager.Pages.Count() - 1) / Options.FieldsPerPage) + 1;
            }
        }

        public SocketCommandContext Context { get; }
        public ICriterion<SocketReaction> Criterion { get; }
        public InteractiveService Interactive { get; }
        public IUserMessage Message { get; private set; }

        private PaginatedAppearanceOptions Options => _pager.Options;
        public RunMode RunMode => RunMode.Sync;
        public TimeSpan? Timeout => Options.Timeout;

        public async Task DisplayAsync()
        {
            var embed = BuildEmbed();
            var message = await Context.Channel.SendMessageAsync(_pager.Content, embed: embed).ConfigureAwait(false);
            Message = message;
            Interactive.AddReactionCallback(message, this);
            // Reactions take a while to add, don't wait for them
            _ = Task.Run(async () =>
            {
                await message.AddReactionAsync(Options.First).ConfigureAwait(false);
                await message.AddReactionAsync(Options.Back).ConfigureAwait(false);
                await message.AddReactionAsync(Options.Next).ConfigureAwait(false);
                await message.AddReactionAsync(Options.Last).ConfigureAwait(false);

                var manageMessages = (Context.Channel is IGuildChannel guildChannel)
                    ? (Context.User as IGuildUser)?.GetPermissions(guildChannel).ManageMessages ?? false
                    : false;

                if (Options.JumpDisplayOptions == JumpDisplayOptions.Always
                    || (Options.JumpDisplayOptions == JumpDisplayOptions.WithManageMessages && manageMessages))
                {
                    await message.AddReactionAsync(Options.Jump).ConfigureAwait(false);
                }

                await message.AddReactionAsync(Options.Stop).ConfigureAwait(false);

                if (Options.DisplayInformationIcon)
                {
                    await message.AddReactionAsync(Options.Info).ConfigureAwait(false);
                }
            });

            // TODO: (Next major version) timeouts need to be handled at the service-level!
            if (Timeout.HasValue && Timeout.Value != null)
            {
                _ = Task.Delay(Timeout.Value).ContinueWith(_ =>
                {
                    Interactive.RemoveReactionCallback(message);
                    _ = Message.DeleteAsync();
                });
            }
        }

        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            var emote = reaction.Emote;

            if (emote.Equals(Options.First))
            {
                _page = 1;
            }
            else if (emote.Equals(Options.Next))
            {
                if (_page >= _pages)
                {
                    return false;
                }

                ++_page;
            }
            else if (emote.Equals(Options.Back))
            {
                if (_page <= 1)
                {
                    return false;
                }

                --_page;
            }
            else if (emote.Equals(Options.Last))
            {
                _page = _pages;
            }
            else if (emote.Equals(Options.Stop))
            {
                await Message.DeleteAsync().ConfigureAwait(false);
                return true;
            }
            else if (emote.Equals(Options.Jump))
            {
                _ = Task.Run(async () =>
                {
                    var criteria = new Criteria<SocketMessage>()
                        .AddCriterion(new EnsureSourceChannelCriterion())
                        .AddCriterion(new EnsureFromUserCriterion(reaction.UserId))
                        .AddCriterion(new EnsureIsIntegerCriterion());
                    var response = await Interactive.NextMessageAsync(Context, criteria, TimeSpan.FromSeconds(15)).ConfigureAwait(false);
                    var request = int.Parse(response.Content);
                    if (request < 1 || request > _pages)
                    {
                        _ = response.DeleteAsync().ConfigureAwait(false);
                        await Interactive.ReplyAndDeleteAsync(Context, Options.Stop.Name).ConfigureAwait(false);
                        return;
                    }
                    _page = request;
                    _ = response.DeleteAsync().ConfigureAwait(false);
                    await RenderAsync().ConfigureAwait(false);
                });
            }
            else if (emote.Equals(Options.Info))
            {
                await Interactive.ReplyAndDeleteAsync(Context, Options.InformationText, timeout: Options.InfoTimeout).ConfigureAwait(false);
                return false;
            }
            _ = Message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
            await RenderAsync().ConfigureAwait(false);
            return false;
        }

        protected virtual Embed BuildEmbed()
        {
            var builder = new EmbedBuilder();

            if (_pager.Pages is IEnumerable<EmbedFieldBuilder> efb)
            {
                builder.Fields = efb.Skip((_page - 1) * Options.FieldsPerPage).Take(Options.FieldsPerPage).ToList();
                builder.Description = _pager.AlternateDescription;
            }
            else if (_pager.Pages.ElementAt(_page - 1) is EmbedBuilder newBuilder)
            {
                // Build and then ToEmbedBuilder to prevent the original Embed being modified
                builder = newBuilder.Build().ToEmbedBuilder();
            }
            else if (_pager.Pages.ElementAt(_page - 1) is Embed newEmbed)
            {
                builder = newEmbed.ToEmbedBuilder();
            }
            else
            {
                // For all other types use the type's own .ToString() function
                builder.Description = _pager.Pages.ElementAt(_page - 1).ToString();
            }

            return builder
                .WithAuthor(builder.Author ?? _pager.Author)
                .WithColor(builder.Color ?? _pager.Color)
                .WithFooter(f => f.Text = $"{builder.Footer.Text}\n{string.Format(Options.FooterFormat, _page, _pages)}".Trim())
                .WithTitle($"{_pager.Title}\n{builder.Title}".Trim())
                .Build();
        }

        private async Task RenderAsync()
        {
            var embed = BuildEmbed();
            await Message.ModifyAsync(m => m.Embed = embed).ConfigureAwait(false);
        }
    }
}