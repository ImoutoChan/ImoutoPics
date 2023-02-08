using ImoutoPicsBot.Data;
using ImoutoPicsBot.Data.Models;
using ImoutoPicsBot.ImageProcessing;
using Telegram.Bot.Types;

namespace ImoutoPicsBot.Cqrs;

public record TryPost : IRequest;

internal class TryPostHandler : IRequestHandler<TryPost>
{
    private readonly int _postEveryHours;
    private readonly int _niceHoursStart;
    private readonly int _niceHoursEnd;
    private readonly long _targetChat;
    
    private readonly IMediaRepository _mediaRepository;
    private readonly IPostInfoRepository _postInfoRepository;
    private readonly IFileStorageRepository _fileStorageRepository;
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly ITelegramPreparer _telegramPreparer;

    public TryPostHandler(
        ITelegramBotClient telegramBotClient,
        IMediaRepository mediaRepository,
        IFileStorageRepository fileStorageRepository,
        IPostInfoRepository postInfoRepository,
        IConfiguration configuration,
        ITelegramPreparer telegramPreparer)
    {
        _telegramBotClient = telegramBotClient;
        _mediaRepository = mediaRepository;
        _fileStorageRepository = fileStorageRepository;
        _postInfoRepository = postInfoRepository;
        _telegramPreparer = telegramPreparer;
        _postEveryHours = configuration.GetValue<int>("PostEveryHours");
        _niceHoursStart = configuration.GetValue<int>("NiceHoursStart");
        _niceHoursEnd = configuration.GetValue<int>("NiceHoursEnd");
        _targetChat = configuration.GetValue<long>("TargetChat");
    }

    public async Task<Unit> Handle(TryPost command, CancellationToken ct)
    {
        var count = _mediaRepository.GetNotPostedCount();
        var lastPostOn = _postInfoRepository.GetLastPostOn();
        var now = DateTimeOffset.UtcNow;

        if (count > 9)
        {
            var toPost = _mediaRepository.GetNotPosted().Take(10).ToList();
            await PostNext(toPost, ct);

            return Unit.Value;
        }

        var timeToPost = now - lastPostOn > TimeSpan.FromHours(_postEveryHours);

        if (timeToPost && IsNiceHour() && count > 0)
        {
            var toPost = _mediaRepository.GetNotPosted().Take(10).ToList();
            await PostNext(toPost, ct);

            return Unit.Value;
        }
        
        return Unit.Value;
    }

    private async Task PostNext(IReadOnlyList<Media> toPost, CancellationToken ct)
    {
        var toPostMediaData = toPost
            .Select((x, i) =>
            {
                var file = _fileStorageRepository.Get(x.Name);
                var preparedFile = _telegramPreparer.Prepare(file.File, file.Size);
                var name = new FileInfo(x.Name).Name;
                return new InputMediaPhoto(new InputMedia(preparedFile, name));
            })
            .OfType<IAlbumInputMedia>()
            .ToList();

        var messages = await _telegramBotClient.SendMediaGroupAsync(_targetChat, toPostMediaData, cancellationToken: ct);

        foreach (var albumInputMedia in toPostMediaData) 
            await albumInputMedia.Media.Content!.DisposeAsync();

        for (var i = 0; i < toPost.Count; i++)
        {
            var post = toPost[i];
            var message = messages[i];

            _mediaRepository.MarkAsPosted(post.Id, message.Chat.Id, message.MessageId);
        }

        _postInfoRepository.SetPosted();

        foreach (var media in toPost) 
            _fileStorageRepository.Delete(media.Name);
    }

    private bool IsNiceHour()
    {
        var hourInMsc = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3)).Hour;
        return hourInMsc >= _niceHoursStart && hourInMsc < _niceHoursEnd;
    }
}
