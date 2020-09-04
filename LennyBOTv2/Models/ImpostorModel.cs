using LiteDB;

namespace LennyBOTv2.Models
{
    public class ImpostorModel
    {
        public ulong Id { get; set; }
        public int Losses { get; set; }
        public string? Nickname { get; set; }

        [BsonIgnore]
        public double Winrate => (double)Wins / (Wins + Losses);

        public int Wins { get; set; }
    }
}
