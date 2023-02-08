using System.Diagnostics;

namespace ImoutoPicsBot.Configuration;

internal class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        IConfiguration configuration)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _logger.LogTrace("Handling {RequestName}", typeof(TRequest).Name);
        try
        {
            var sw = new Stopwatch();
            sw.Start();
            var response = await next();
            sw.Stop();

            _logger.LogInformation(
                "Handled {RequestName} with response {ResponseName} in {HandleTime} ms",
                typeof(TRequest).Name,
                typeof(TResponse).Name,
                sw.ElapsedMilliseconds);

            return response;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in handling {RequestName}", typeof(TRequest).Name);
            throw;
        }
    }
}
