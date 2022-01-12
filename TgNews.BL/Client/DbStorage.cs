using LiteDB;

namespace TgNews.BL.Client;

// https://github.com/mbdavid/LiteDB/wiki/Getting-Started
public class DbStorage
{
    private readonly string _dbFile;
    private readonly string _keyValueCollection;

    public DbStorage(TgNewsConfiguration cfg)
    {
        _dbFile = cfg.DbFile;
        _keyValueCollection = "Key_Value";
    }

    public class KeyValue<T>
    {
        public ObjectId Id { get; set; }
        public string Key { get; set; }
        public T Value { get; set; }
    }
    
    public T ReadKey<T>(string key)
    {
        using var db = new LiteDatabase(_dbFile);
        var col = db.GetCollection<KeyValue<T>>(_keyValueCollection);
        var entity = col.FindOne(x => x.Key == key);
        return entity == null ? default(T) : entity.Value;
    }

    public void SetKey<T>(string key, T value)
    {
        using var db = new LiteDatabase(_dbFile);
        
        // Get a collection (or create, if doesn't exist)
        var col = db.GetCollection<KeyValue<T>>(_keyValueCollection);
        col.EnsureIndex(c => c.Key);

        var entity = col.FindOne(x => x.Key == key);
        if (entity == null)
        {
            entity = new KeyValue<T>() { Key = key, Value = value, };
            col.Insert(entity);
            return;
        }
        
        entity.Value = value;
        col.Update(entity);
    }
}