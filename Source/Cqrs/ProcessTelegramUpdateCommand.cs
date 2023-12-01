using ImoutoPicsBot.Configuration;
using ImoutoPicsBot.Data;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ImoutoPicsBot.Cqrs;

public record ProcessTelegramUpdateCommand(Update Update) : IRequest;

internal class ProcessTelegramUpdateCommandHandler : IRequestHandler<ProcessTelegramUpdateCommand>
{
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly int _moderator;
    private readonly IMediaRepository _mediaRepository;
    private readonly IPostInfoRepository _postInfoRepository;

    public ProcessTelegramUpdateCommandHandler(
        ITelegramBotClient telegramBotClient,
        IConfiguration configuration,
        IMediaRepository mediaRepository,
        IPostInfoRepository postInfoRepository)
    {
        _moderator = configuration.GetRequiredValue<int>("ModeratorId");
        _telegramBotClient = telegramBotClient;
        _mediaRepository = mediaRepository;
        _postInfoRepository = postInfoRepository;
    }

    public async Task Handle(ProcessTelegramUpdateCommand command, CancellationToken ct)
    {
        var update = command.Update;

        if (update.Type == UpdateType.Message)
        {
            var message = update.Message!;
            var allowed = message.From!.Id == _moderator;

            if (!allowed)
                return;

            var count = _mediaRepository.GetNotPostedCount();
            var lastPostOn = _postInfoRepository.GetLastPostOn();

            await _telegramBotClient.SendTextMessageAsync(
                message.Chat, 
                $"""
                Кира кира!
                Последний пост был в {lastPostOn.ToOffset(TimeSpan.FromHours(4))}
                Накопилось {count} файлов
                """,
                cancellationToken: ct);
        }
    }
}
