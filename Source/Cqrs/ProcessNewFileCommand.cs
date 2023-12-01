using ImoutoPicsBot.Data;

namespace ImoutoPicsBot.Cqrs;

public record ProcessNewFileCommand(Stream File, string Filename) : IRequest;

internal class ProcessNewFileCommandHandler : IRequestHandler<ProcessNewFileCommand>
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IFileStorageRepository _fileStorageRepository;
    private readonly IMediator _mediator;
    private readonly ITelegramBotClient _telegramBotClient;

    public ProcessNewFileCommandHandler(
        ITelegramBotClient telegramBotClient,
        IMediator mediator,
        IMediaRepository mediaRepository,
        IFileStorageRepository fileStorageRepository)
    {
        _telegramBotClient = telegramBotClient;
        _mediator = mediator;
        _mediaRepository = mediaRepository;
        _fileStorageRepository = fileStorageRepository;
    }

    public async Task Handle(ProcessNewFileCommand command, CancellationToken ct)
    {
        var file = command.File;
        var name = command.Filename;

        var savedFile = await _fileStorageRepository.Save(file, name, ct);
        _mediaRepository.Add(savedFile.LocalFileName, savedFile.Md5);
    }
}
