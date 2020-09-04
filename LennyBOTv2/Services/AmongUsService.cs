using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using LennyBOTv2.Models;
using LiteDB;

namespace LennyBOTv2.Services
{
    public class AmongUsService
    {
        public IEnumerable<ImpostorModel> GetStats()
        {
            using (var db = LennyServiceProvider.OpenDB())
            {
                return db.GetCollection<ImpostorModel>().FindAll().ToList();
            }
        }

        public void WriteStats(IUser player, string gameResult)
        {
            bool result;
            if (gameResult.StartsWith("W", StringComparison.OrdinalIgnoreCase))
            {
                result = true;
            }
            else if (gameResult.StartsWith("L", StringComparison.OrdinalIgnoreCase))
            {
                result = false;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(gameResult), gameResult, "Invalid value");
            }

            using (var db = LennyServiceProvider.OpenDB())
            {
                var impostors = db.GetCollection<ImpostorModel>();

                var impostor = impostors.FindOne(i => i.Id == player.Id) ?? new ImpostorModel() { Id = player.Id };
                impostor.Nickname = player.GetNickname();

                _ = result ? impostor.Wins++ : impostor.Losses++;

                impostors.Upsert(impostor);
            }
        }
    }
}
