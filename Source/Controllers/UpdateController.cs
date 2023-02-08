using ImoutoPicsBot.Cqrs;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace ImoutoPicsBot.Controllers;

[ApiController]
[Route("imouto-pics/update")]
public class UpdateController : ControllerBase
{
    private readonly IMediator _mediator;

    public UpdateController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public Task Post([FromBody] Update update) => _mediator.Send(new ProcessTelegramUpdateCommand(update));
}
