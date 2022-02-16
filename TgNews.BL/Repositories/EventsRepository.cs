using LiteDB;

namespace TgNews.BL.Repositories;

// https://github.com/mbdavid/LiteDB/wiki/Getting-Started
public class EventRepository
{
    private readonly string _dbFile;
    private readonly string _collection;

    public EventRepository(TgNewsConfiguration cfg)
    {
        _dbFile = cfg.DbFile;
        _collection = "events";
    }

    public class EventRecord
    {
        public ObjectId? Id { get; set; }
        public DateTime Received { get; set; }
        public string Type { get; set; }
        public string Json { get; set; }
    }

    public void Insert(EventRecord entity)
    {
        using var db = new LiteDatabase(_dbFile);
        
        // Get a collection (or create, if doesn't exist)
        var col = db.GetCollection<EventRecord>(_collection);
        col.EnsureIndex(c => c.Received);
        col.Insert(entity);
    }
}