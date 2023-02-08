using System.Security.Cryptography;

namespace ImoutoPicsBot.Data;

internal class FileStorageRepository : IFileStorageRepository
{
    public (Stream File, long Size) Get(string localFileName)
    {
        var info = new FileInfo(localFileName);
        return (info.OpenRead(), info.Length);
    }

    public IReadOnlyCollection<string> GetStored()
    {
        var dir = new DirectoryInfo(Path.Combine("idata", "files"));
        return dir.GetFiles().Select(x => x.FullName).ToList();
    }

    public void Delete(string localFileName) => File.Delete(localFileName);

    public async Task<(string LocalFileName, string Md5)> Save(Stream file, string name, CancellationToken ct)
    {
        using var cachedFile = new MemoryStream();
        await file.CopyToAsync(cachedFile, ct);
        cachedFile.Seek(0, SeekOrigin.Begin);
        await file.DisposeAsync();

        var md5 = CalculateMd5(cachedFile);
        var extension = name.Split('.').Last();
        var newName = md5 + "." + extension;

        var newFile = new FileInfo(Path.Combine("idata", "files", newName));
        newFile.Directory?.Create();
        
        if (newFile.Exists)
            return (newFile.FullName, md5);

        await using var write = newFile.Create();
        await cachedFile.CopyToAsync(write, ct);

        return (newFile.FullName, md5);
    }

    private static string CalculateMd5(Stream file)
    {
        using var md5 = MD5.Create();

        var hashBytes = md5.ComputeHash(file);
        file.Seek(0, SeekOrigin.Begin);

        return string.Join("", hashBytes.Select(x => x.ToString("X2"))).ToLower();
    }
}
