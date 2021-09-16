using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using LennyBOTv2.Models.AmongUs;
using LiteDB;

namespace LennyBOTv2.Services
{
    public class AmongUsService
    {
        public Embed GetStatsEmbed(string collectionName)
        {
            List<PlayerModel> collection;
            using (var db = LennyServiceProvider.OpenDB())
            {
                collection = db.GetCollection<PlayerModel>(collectionName).FindAll().OrderByDescending(i => i.Winrate).ToList();
            }

            var sb = new StringBuilder();

            for (var i = 0; i < collection.Count; i++)
            {
                var impostor = collection[i];

                var place = i switch
                {
                    0 => ":first_place:",
                    1 => ":second_place:",
                    2 => ":third_place:",
                    _ => $"**{i + 1}**",
                };

                sb.AppendFormat("{0} {1} {2}/{3} ({4:P2})", place, impostor.Nickname, impostor.Wins, impostor.Wins + impostor.Losses, impostor.Winrate).AppendLine();
            }

            var embed = new EmbedBuilder()
                    .WithColor(new Color(255, 0, 0))
                    .WithCurrentTimestamp()
                    .WithTitle($"{collectionName} stats")
                    .WithDescription(sb.ToString())
                    .WithThumbnailUrl("https://i.imgur.com/vdPEmSE.png");

            return embed.Build();
        }

        public void WriteCrewStats(string gameResult, IUser[] players)
        {
            WriteStats("crew", gameResult, players);
        }

        public void WriteImpostorStats(string gameResult, IUser[] players)
        {
            WriteStats("impostor", gameResult, players);
        }

        private IEnumerable<PlayerModel> GetStats(string collectionName)
        {
            using (var db = LennyServiceProvider.OpenDB())
            {
                return db.GetCollection<PlayerModel>(collectionName).FindAll().ToList();
            }
        }

        private void WriteStats(string collectionName, string gameResult, IUser[] players)
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
                var collection = db.GetCollection<PlayerModel>(collectionName);

                foreach (var player in players)
                {
                    var entity = collection.FindOne(i => i.Id == player.Id) ?? new PlayerModel() { Id = player.Id };
                    entity.Nickname = player.GetNickname();

                    _ = result ? entity.Wins++ : entity.Losses++;

                    collection.Upsert(entity);
                }
            }
        }
    }
}
