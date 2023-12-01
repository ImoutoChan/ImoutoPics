using ImoutoPicsBot.Data;

namespace ImoutoPicsBot.Cqrs;

public record CleanupFiles : IRequest;

internal class CleanupFilesHandler : IRequestHandler<CleanupFiles>
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IFileStorageRepository _fileStorageRepository;

    public CleanupFilesHandler(
        IMediaRepository mediaRepository,
        IFileStorageRepository fileStorageRepository)
    {
        _mediaRepository = mediaRepository;
        _fileStorageRepository = fileStorageRepository;
    }

    public Task Handle(CleanupFiles command, CancellationToken ct)
    {
        var stored = _fileStorageRepository.GetStored();

        foreach (var storedFile in stored)
        {
            var media = _mediaRepository.GetByName(storedFile);

            if (media == null)
                continue;
            
            if (media.IsPosted)
                _fileStorageRepository.Delete(storedFile);
        }
        
        return Task.CompletedTask;
    }
}
