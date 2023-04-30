using ImoutoPicsBot.Data.Models;
using LiteDB;

namespace ImoutoPicsBot.Data;

internal class PostInfoRepository : IPostInfoRepository
{
    private readonly ILiteDatabase _database;

    public PostInfoRepository(ILiteDatabase database) => _database = database;


    public DateTimeOffset GetLastPostOn()
    {
        var collection = GetCollection();

        return collection.Query().FirstOrDefault()?.LastPostedOn ?? DateTimeOffset.MinValue;
    }

    public void SetPosted()
    {
        var collection = GetCollection();

        var found = collection.Query().FirstOrDefault();

        if (found == null)
        {
            collection.Insert(new PostsInfo()
            {
                LastPostedOn = DateTimeOffset.UtcNow
            });
        }
        else
        {
            found.LastPostedOn = DateTimeOffset.UtcNow;
            collection.Update(found);
        }
    }

    private ILiteCollection<PostsInfo> GetCollection() => _database.GetCollection<PostsInfo>("info");
}
