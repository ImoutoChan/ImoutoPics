using ImoutoPicsBot.Configuration;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ImoutoPicsBot.Cqrs;

public record ProcessTelegramUpdateCommand(Update Update) : IRequest;

internal class ProcessTelegramUpdateCommandHandler : IRequestHandler<ProcessTelegramUpdateCommand>
{
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly int _moderator;

    public ProcessTelegramUpdateCommandHandler(ITelegramBotClient telegramBotClient, IConfiguration configuration)
    {
        _moderator = configuration.GetRequiredValue<int>("ModeratorId");
        _telegramBotClient = telegramBotClient;
    }

    public async Task<Unit> Handle(ProcessTelegramUpdateCommand command, CancellationToken ct)
    {
        var update = command.Update;

        if (update.Type == UpdateType.Message)
        {
            var message = update.Message!;
            var allowed = message.From!.Id == _moderator;

            if (!allowed)
                return Unit.Value;

            await _telegramBotClient.SendTextMessageAsync(message.Chat, "Мяу!", cancellationToken: ct);
        }

        return Unit.Value;
    }
}
