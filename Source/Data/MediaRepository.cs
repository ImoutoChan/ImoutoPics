using ImoutoPicsBot.Data.Models;
using LiteDB;

namespace ImoutoPicsBot.Data;

internal class MediaRepository : IMediaRepository
{
    private readonly ILiteDatabase _database;

    public MediaRepository(ILiteDatabase database) => _database = database;

    public IReadOnlyCollection<Media> GetNotPosted()
    {
        var collection = GetCollection();

        return collection.Query().Where(x => !x.IsPosted).ToList();
    }

    public Media? GetByName(string fullFileName)
    {
        var collection = GetCollection();

        return collection.Query().Where(x => x.Name == fullFileName).FirstOrDefault();
    }

    public int GetNotPostedCount()
    {
        var collection = GetCollection();

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

        var collection = GetCollection();

        var existingMedia = collection.Query().Where(x => x.Md5 == md5).FirstOrDefault();
        if (existingMedia != null)
            return;

        collection.Insert(newMedia);
    }

    public void MarkAsPosted(int id, long chatId, int messageId)
    {
        var collection = GetCollection();

        var item = collection.Query().Where(x => x.Id == id).FirstOrDefault();
        item.IsPosted = true;
        item.PostedChat = chatId;
        item.PostedMessageId = messageId;
        item.PostedOn = DateTimeOffset.UtcNow;
        collection.Update(item);
    }

    private ILiteCollection<Media> GetCollection() => _database.GetCollection<Media>("media");
}
