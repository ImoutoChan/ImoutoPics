using ImoutoPicsBot.Configuration;
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
    private readonly string _targetChat;
    
    private readonly IMediaRepository _mediaRepository;
    private readonly IPostInfoRepository _postInfoRepository;
    private readonly IFileStorageRepository _fileStorageRepository;
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly ITelegramPreparer _telegramPreparer;
    private readonly ILogger<TryPostHandler> _logger;

    public TryPostHandler(
        ITelegramBotClient telegramBotClient,
        IMediaRepository mediaRepository,
        IFileStorageRepository fileStorageRepository,
        IPostInfoRepository postInfoRepository,
        IConfiguration configuration,
        ITelegramPreparer telegramPreparer,
        ILogger<TryPostHandler> logger)
    {
        _telegramBotClient = telegramBotClient;
        _mediaRepository = mediaRepository;
        _fileStorageRepository = fileStorageRepository;
        _postInfoRepository = postInfoRepository;
        _telegramPreparer = telegramPreparer;
        _logger = logger;
        _postEveryHours = configuration.GetValue<int>("PostEveryHours");
        _niceHoursStart = configuration.GetValue<int>("NiceHoursStart");
        _niceHoursEnd = configuration.GetValue<int>("NiceHoursEnd");
        _targetChat = configuration.GetRequiredValue<string>("TargetChat");
    }

    public async Task Handle(TryPost command, CancellationToken ct)
    {
        var count = _mediaRepository.GetNotPostedCount();
        var lastPostOn = _postInfoRepository.GetLastPostOn();
        var now = DateTimeOffset.UtcNow;

        if (count > 9)
        {
            var toPost = _mediaRepository.GetNotPosted().Take(10).ToList();
            await PostNext(toPost, ct);

            return;
        }

        var timeToPost = now - lastPostOn > TimeSpan.FromHours(_postEveryHours);

        if (timeToPost && IsNiceHour() && count > 0)
        {
            var toPost = _mediaRepository.GetNotPosted().Take(10).ToList();
            await PostNext(toPost, ct);
        }
    }

    private async Task PostNext(IReadOnlyList<Media> toPost, CancellationToken ct)
    {
        var toDispose = new List<Stream>();
        
        var toPostMediaData = toPost
            .Select(x =>
            {
                try
                {
                    var type = GetType(x.Name);
                    
                    var file = _fileStorageRepository.Get(x.Name);
                    
                    var preparedFile = type == MediaType.Photo
                        ? _telegramPreparer.Prepare(file.File, file.Size)
                        : file.File;
                    
                    toDispose.Add(preparedFile);
                    
                    var name = new FileInfo(x.Name).Name;
                    InputMediaPhoto? media = new InputMediaPhoto(new InputFileStream(preparedFile, name));

                    return (TgMedia: media, MediaMetadata: x);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to post the image {Name}", x.Name);
                    return (TgMedia: (InputMediaPhoto?)null, MediaMetadata: x);
                }
            })
            .ToList();

        var album = toPostMediaData
            .Where(x => x.TgMedia != null)
            .Select(x => x.TgMedia)
            .OfType<IAlbumInputMedia>()
            .ToList();

        var messages = await _telegramBotClient.SendMediaGroupAsync(
            _targetChat, 
            album, 
            cancellationToken: ct);

        foreach (var x in toDispose)
            await x.DisposeAsync();

        for (var i = 0; i < toPostMediaData.Count; i++)
        {
            var post = toPostMediaData[i];
            var message = messages[i];
            
            if (post.TgMedia == null)
                continue;

            _mediaRepository.MarkAsPosted(post.MediaMetadata.Id, message.Chat.Id, message.MessageId);
        }

        _postInfoRepository.SetPosted();

        foreach (var post in toPostMediaData)
        {
            if (post.TgMedia == null)
                continue;
            
            _fileStorageRepository.Delete(post.MediaMetadata.Name);
        }
    }

    private bool IsNiceHour()
    {
        var hourInMsc = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3)).Hour;
        return hourInMsc >= _niceHoursStart && hourInMsc < _niceHoursEnd;
    }
    
    private static MediaType GetType(string path) =>
        path.EndsWith(".mp4") || path.EndsWith(".webm") || path.EndsWith(".swf")
            ? MediaType.Video
            : MediaType.Photo;
    
    private enum MediaType
    {
        Unknown,
        Photo,
        Video
    }
}

