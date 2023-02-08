using Newtonsoft.Json;

namespace ImoutoPicsBot.Configuration;

public class TelegramHostedService : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TelegramHostedService> _logger;
    private readonly ITelegramBotClient _telegramBotClient;

    public TelegramHostedService(
        ILogger<TelegramHostedService> logger,
        IConfiguration configuration,
        ITelegramBotClient telegramBotClient)
    {
        _logger = logger;
        _configuration = configuration;
        _telegramBotClient = telegramBotClient;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        var address = _configuration.GetRequiredValue<string>("WebhookAddress");

        _logger.LogInformation("Removing Webhook");
        await _telegramBotClient.DeleteWebhookAsync(cancellationToken: ct);

        _logger.LogInformation("Setting Webhook to {Address}", address);
        await _telegramBotClient.SetWebhookAsync(
            address,
            maxConnections: 5,
            cancellationToken: ct);
        _logger.LogInformation("Webhook is set to {Address}", address);

        var webHookInfo = await _telegramBotClient.GetWebhookInfoAsync(ct);
        _logger.LogInformation("Webhook info: {Info}", JsonConvert.SerializeObject(webHookInfo));
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _telegramBotClient.DeleteWebhookAsync(cancellationToken: cancellationToken);
        _logger.LogInformation("Webhook removed");
    }
}
