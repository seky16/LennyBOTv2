using System;
using System.Threading.Tasks;
using Discord.Commands;
using LennyBOTv2.Services;

namespace LennyBOTv2.Modules
{
    public class ReminderModule :LennyModuleBase
    {
        private readonly ReminderService _rs;

        public ReminderModule(ReminderService rs)
        {
            _rs = rs;
        }

        [Command("remindme")]
        public async Task RemindMeCmdAsync([Remainder] string input)
        {
            try
            {
                _rs.CreateReminder(Context.Message, input);
            }
            catch (Exception ex)
            {
                await Context.MarkCmdFailedAsync(ex.ToString());
                return;
            }

            await Context.MarkCmdOkAsync().ConfigureAwait(false);
        }
    }
}
