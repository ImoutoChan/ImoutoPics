using ImoutoPicsBot.Cqrs;
using Quartz;

namespace ImoutoPicsBot.QuartzJobs;

internal sealed class PostingJob : IJob
{
    private readonly IMediator _mediator;
    private static readonly SemaphoreSlim Locker = new(1);

    public PostingJob(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        if (!await Locker.WaitAsync(0))
            return;

        try
        {
            await _mediator.Send(new TryPost());
            await _mediator.Send(new CleanupFiles());
        }
        finally
        {
            Locker.Release();
        }
    }
}
