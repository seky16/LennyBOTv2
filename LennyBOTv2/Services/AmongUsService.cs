using System.Threading.Tasks;
using Discord;

namespace LennyBOTv2.Services
{
    public class AmongUsService
    {
        public Task WriteStatsAsync(IUser player, string gameResult)
        {
            //throw new NotImplementedException();
            LoggingService.LogInfoAsync(player.GetNickname());
            return Task.CompletedTask;
        }

        internal Task GetStats()
        {
            //throw new NotImplementedException();
            return Task.CompletedTask;
        }
    }
}
