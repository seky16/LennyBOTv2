using System;
using LiteDB;
using NodaTime;

namespace LennyBOTv2.Services
{
    public static class CacheService
    {
        #region Common

        public static BsonValue? GetCacheEntry(string key)
        {
            using var db = LennyServiceProvider.OpenDB();
            return db.GetCollection("cache").FindById(key)?["entry"];
        }

        public static void SetCacheEntry(string key, object entry)
        {
            using var db = LennyServiceProvider.OpenDB();
            db.GetCollection("cache").Upsert(key, new BsonDocument() { ["entry"] = new BsonValue(entry) });
        }

        #endregion Common

        #region TimerService

        public static int TimerService_Eta
        {
            get => GetCacheEntry(nameof(TimerService_Eta))?.AsInt32 ?? 0;
            set => SetCacheEntry(nameof(TimerService_Eta), value);
        }

        public static LocalDate TimerService_LastSentFrogMsg
        {
            get => GetCacheEntry(nameof(TimerService_LastSentFrogMsg))?.AsDateTime.UtcToPragueZonedDateTime().Date ?? LocalDate.FromDateTime(DateTime.UtcNow);
            set => SetCacheEntry(nameof(TimerService_LastSentFrogMsg), DateTime.SpecifyKind(value.ToDateTimeUnspecified(), DateTimeKind.Utc));
        }

        #endregion TimerService
    }
}
