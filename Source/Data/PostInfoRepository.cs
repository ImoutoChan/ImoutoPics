using ImoutoPicsBot.Data.Models;
using LiteDB;

namespace ImoutoPicsBot.Data;

internal class PostInfoRepository : IPostInfoRepository
{
    private readonly Func<ILiteDatabase> _getDatabase;

    public PostInfoRepository(Func<ILiteDatabase> getDatabase) => _getDatabase = getDatabase;
    
    public DateTimeOffset GetLastPostOn()
    {
        using var db = _getDatabase();
        var collection = GetCollection(db);

        return collection.Query().FirstOrDefault()?.LastPostedOn ?? DateTimeOffset.MinValue;
    }

    public void SetPosted()
    {
        using var db = _getDatabase();
        var collection = GetCollection(db);

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

    private static ILiteCollection<PostsInfo> GetCollection(ILiteDatabase db) => db.GetCollection<PostsInfo>("info");
}
