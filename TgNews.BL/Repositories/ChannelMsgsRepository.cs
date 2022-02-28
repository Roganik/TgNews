using LiteDB;

namespace TgNews.BL.Repositories;

// https://github.com/mbdavid/LiteDB/wiki/Getting-Started
public class ChannelMsgsRepository
{
    private readonly string _dbFile;
    private readonly string _collection;

    public ChannelMsgsRepository(TgNewsConfiguration cfg)
    {
        _dbFile = cfg.DbFile;
        _collection = "channelmsgs";
    }

    public class MsgRecord
    {
        public ObjectId? Id { get; set; }
        public DateTime Received { get; set; }

        public string Channel { get; set; }

        public string? Msg { get; set; }
        public string? Json { get; set; }
    }

    public void Insert(string channel, string text, DateTime eventDate, string eventJson)
    {
        var entity = new MsgRecord()
        {
            Channel = channel,
            Json = eventJson,
            Received = eventDate,
            Msg = text,
        };

        using var db = new LiteDatabase(_dbFile);
        
        // Get a collection (or create, if doesn't exist)
        var col = db.GetCollection<MsgRecord>(_collection);
        col.EnsureIndex(c => c.Received);
        col.Insert(entity);
    }
}