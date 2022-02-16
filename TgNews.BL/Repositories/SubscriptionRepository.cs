using LiteDB;

namespace TgNews.BL.Repositories;

// https://github.com/mbdavid/LiteDB/wiki/Getting-Started
public class SubscriptionRepository
{
    private readonly string _dbFile;
    private readonly string _collection;

    public SubscriptionRepository(TgNewsConfiguration cfg)
    {
        _dbFile = cfg.DbFile;
        _collection = "subscriptions";
    }

    public class SubscriptionRecord
    {
        public ObjectId? Id { get; set; }
        public string ChannelName { get; set; }
        public int TgLastReadMsgId { get; set; }
        public int TgLastForwardedMsgId { get; set; }
        public long TgChannelId { get; set; }
    }

    public SubscriptionRecord Get(string channelName)
    {
        using var db = new LiteDatabase(_dbFile);
        var col = db.GetCollection<SubscriptionRecord>(_collection);
        var entity = col.FindOne(x => x.ChannelName == channelName);
        return entity ?? new SubscriptionRecord()
        {
            ChannelName = channelName,
        };
    }

    public void Update(SubscriptionRecord entity)
    {
        using var db = new LiteDatabase(_dbFile);
        
        // Get a collection (or create, if doesn't exist)
        var col = db.GetCollection<SubscriptionRecord>(_collection);
        col.EnsureIndex(c => c.ChannelName);

        if (entity.Id == null)
        {
            col.Insert(entity);
            return;
        }

        col.Update(entity);
    }
}