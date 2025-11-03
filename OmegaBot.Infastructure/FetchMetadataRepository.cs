using LiteDB;
using OmegaBot.Application.Interfaces;
using OmegaBot.Domain.Models;

namespace OmegaBot.Infastructure
{
    public class FetchMetadataRepository : IFetchMetadataRepository
    {
        private readonly string _connectionString;

        public FetchMetadataRepository(AppSettings appSettings)
        {
            _connectionString = appSettings.ConnectionString;
        }

        public void AddFetchRecord(FetchRecord fetchRecord)
        {
            using var db = new LiteDatabase(_connectionString);
            var fetchRecords = db.GetCollection<FetchRecord>("FetchRecords");
            fetchRecords.Upsert(fetchRecord);
        }

        public long GetLatestFetchRecordTimestamp()
        {
            using var db = new LiteDatabase(_connectionString);
            var fetchRecords = db.GetCollection<FetchRecord>("FetchRecords");
            fetchRecords.EnsureIndex(x => x.CreatedAt);

            if(fetchRecords.Count() == 0)
            {
                return ((DateTimeOffset)DateTime.UnixEpoch).ToUnixTimeSeconds();
            }

            return fetchRecords.FindOne(Query.All("CreatedAt", Query.Descending)).Timestamp;
        }
    }
}
