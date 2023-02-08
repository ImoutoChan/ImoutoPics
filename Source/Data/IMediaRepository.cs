using ImoutoPicsBot.Data.Models;

namespace ImoutoPicsBot.Data;

public interface IMediaRepository
{
    IReadOnlyCollection<Media> GetNotPosted();
    
    Media? GetByName(string fullFileName);
    
    void Add(string name, string md5);

    void MarkAsPosted(int id, long chatId, int messageId);
    
    int GetNotPostedCount();
}
