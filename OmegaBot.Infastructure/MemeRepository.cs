using LiteDB;
using OmegaBot.Application.Interfaces;
using OmegaBot.Domain.Models;
using OmegaBot.Services;

namespace OmegaBot.Infastructure
{
    public class MemeRepository : IMemeRepository
    {
        private readonly string _connectionString;

        public MemeRepository(AppSettings appSettings)
        {
            _connectionString = appSettings.ConnectionString;
        }

        public void Initialize()
        {
            using var db = new LiteDatabase(_connectionString);

            var memes = db.GetCollection<Meme>("Memes");

            memes.EnsureIndex(x => x.Id, unique: true);
            memes.EnsureIndex(x => x.AuthorId, unique: false);
            memes.EnsureIndex(x => x.ChannelId, unique: false);
            memes.EnsureIndex(x => x.FileName, unique: true);
            memes.EnsureIndex(x => x.IsBanned, unique: false);            
        }

        public void AddBulk(IReadOnlyCollection<Meme> memes, int batchSize = 100000)
        {
            var batches = memes.Select((item, idx) => (item, idx))
                .GroupBy(x => x.idx / batchSize)
                .Select(g => g.Select(p => p.item).ToList());

            using var db = new LiteDatabase(_connectionString);
            var col = db.GetCollection<Meme>("Memes");

            foreach (var batch in batches)
            {
                db.BeginTrans();
                try
                {
                    col.Insert(batch);
                    db.Commit();
                }
                catch
                {
                    db.Rollback();
                    throw;
                }
            }
        }

        public Meme BanMeme(int memeId)
        {
            using var db = new LiteDatabase(_connectionString);
            var memes = db.GetCollection<Meme>("Memes");

            var meme = memes.FindOne(x => x.Id == memeId);
            if(!meme.IsBanned)
            {
                meme.IsBanned = true;
                memes.Update(meme);
            }

            return meme;
        }

        public ulong GetAuthorId(string fileName)
        {
            using var db = new LiteDatabase(_connectionString);
            var memes = db.GetCollection<Meme>("Memes");
            return memes.FindOne(x => x.FileName == fileName).AuthorId;
        }

        public IReadOnlyCollection<Meme> GetBannedMemes()
        {
            using var db = new LiteDatabase(_connectionString);
            var memes = db.GetCollection<Meme>("Memes");
            return memes
                .Find(x => x.IsBanned)
                .ToList();
        }

        public Meme GetRandomMemeInChannel(ulong channelId, bool allowBanned = false)
        {
            using var db = new LiteDatabase(_connectionString);
            var memes = db.GetCollection<Meme>("Memes");

            int count = memes.Count(x => x.ChannelId == channelId);
            if (count == 0) return null;

            var skip = RandomNumbersProvider.GetRandomNumberInRange(0, count);

            return memes.Find(x => x.ChannelId == channelId, skip, 1).FirstOrDefault();
        }

        public int GetTotalMemeCount(ulong? channelId = null, ulong? authorId = null)
        {
            using var db = new LiteDatabase(_connectionString);
            var memes = db.GetCollection<Meme>("Memes");

            return memes.Count(x =>
                 (channelId == null || x.ChannelId == channelId) &&
                 (authorId == null || (x.ContainsMention ? x.MentionedUserId == authorId : x.AuthorId == authorId)));
        }

        public IReadOnlyDictionary<ulong, int> GetMemeCountPerChannel()
        {
            using var db = new LiteDatabase(_connectionString);
            var memes = db.GetCollection<BsonDocument>("Memes");

            var groups = memes.Query()
                .GroupBy("ChannelId")
                .Select("{Key: @key,Count: Count(*)}")
                .ToList()
                .OrderByDescending(d => d["Count"].AsInt32)
                .ToList();

            return groups.ToDictionary(
                    d => (ulong)d["Key"].AsInt64,
                    d => d["Count"].AsInt32
                );
        }
    }
}
