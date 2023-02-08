namespace ImoutoPicsBot.Data;

internal interface IFileStorageRepository
{
    (Stream File, long Size) Get(string localFileName);
    
    IReadOnlyCollection<string> GetStored();
    
    Task<(string LocalFileName, string Md5)> Save(Stream file, string name, CancellationToken ct);
    
    void Delete(string localFileName);
}
