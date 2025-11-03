using LiteDB;
using OmegaBot.Application.Interfaces;
using OmegaBot.Domain.Models;
using System.Diagnostics.Metrics;

namespace OmegaBot.Infastructure
{
    public class BanRecordEventsRepository : IBanRecordEventsRepository
    {
        private readonly string _connectionString;

        public BanRecordEventsRepository(AppSettings appSettings)
        {
            _connectionString = appSettings.ConnectionString;
        }

        public BanRecordEvent AddBanRecord(BanRecordEvent fetchRecord)
        {
            using var db = new LiteDatabase(_connectionString);
            var banRecordEvents = db.GetCollection<BanRecordEvent>("BanRecordEvents");
            banRecordEvents.Upsert(fetchRecord);
            return fetchRecord;
        }

        public BanRecordEvent GetFastestBanRecord()
        {
            using var db = new LiteDatabase(_connectionString);
            var banRecordEvents = db.GetCollection<BanRecordEvent>("BanRecordEvents");
            banRecordEvents.EnsureIndex(x => x.TimeToBan);
            return banRecordEvents.Query()
                            .OrderBy("TimeToBan")
                            .Limit(1)
                            .ToList()
                            .FirstOrDefault();
        }
    }
}
