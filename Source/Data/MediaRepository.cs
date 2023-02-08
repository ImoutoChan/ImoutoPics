using ImoutoPicsBot.Data.Models;
using LiteDB;

namespace ImoutoPicsBot.Data;

internal class MediaRepository : IMediaRepository
{
    private readonly Func<ILiteDatabase> _getDatabase;

    public MediaRepository(Func<ILiteDatabase> getDatabase) => _getDatabase = getDatabase;

    public IReadOnlyCollection<Media> GetNotPosted()
    {
        using var db = _getDatabase();
        var collection = GetCollection(db);

        return collection.Query().Where(x => !x.IsPosted).ToList();
    }

    public Media? GetByName(string fullFileName)
    {
        using var db = _getDatabase();
        var collection = GetCollection(db);

        return collection.Query().Where(x => x.Name == fullFileName).FirstOrDefault();
    }

    public int GetNotPostedCount()
    {
        using var db = _getDatabase();
        var collection = GetCollection(db);

        return collection.Query().Where(x => !x.IsPosted).Count();
    }

    public void Add(string name, string md5)
    {
        var newMedia = new Media
        {
            Name = name,
            Md5 = md5,
            IsPosted = false,
            PostedChat = null,
            PostedMessageId = null,
            AddedOn = DateTimeOffset.UtcNow,
            PostedOn = null
        };

        using var db = _getDatabase();
        var collection = GetCollection(db);

        var existingMedia = collection.Query().Where(x => x.Md5 == md5).FirstOrDefault();
        if (existingMedia != null)
            return;

        collection.Insert(newMedia);
    }

    public void MarkAsPosted(int id, long chatId, int messageId)
    {
        using var db = _getDatabase();
        var collection = GetCollection(db);

        var item = collection.Query().Where(x => x.Id == id).FirstOrDefault();
        item.IsPosted = true;
        item.PostedChat = chatId;
        item.PostedMessageId = messageId;
        item.PostedOn = DateTimeOffset.UtcNow;
        collection.Update(item);
    }

    private static ILiteCollection<Media> GetCollection(ILiteDatabase db) => db.GetCollection<Media>("media");
}
