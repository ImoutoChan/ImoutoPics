namespace ImoutoPicsBot.Data;

public interface IPostInfoRepository
{
    DateTimeOffset GetLastPostOn();

    void SetPosted();
}