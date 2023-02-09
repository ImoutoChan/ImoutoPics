using ImoutoPicsBot.Configuration;
using ImoutoPicsBot.Cqrs;
using Microsoft.AspNetCore.Mvc;

namespace ImoutoPicsBot.Controllers;

[ApiController]
[Route("imouto-pics/upload")]
public class UploadController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UploadController> _logger;
    private readonly string _securityString;

    public UploadController(IMediator mediator, ILogger<UploadController> logger, IConfiguration configuration)
    {
        _mediator = mediator;
        _logger = logger;
        _securityString = configuration.GetRequiredValue<string>("SecurityString");
    }

    [HttpPost]
    public async Task Post(IEnumerable<IFormFile> files, [FromQuery] string security)
    {
        if (_securityString == "" || security != _securityString)
        {
            _logger.LogWarning("Attempt to upload a file with security {Security}", security);
            return;
        }
        
        if (!files.Any())
        {
            _logger.LogWarning("Empty files");
            return;
        }
        
        foreach (var file in files)
        {
            try
            {
                await _mediator.Send(new ProcessNewFileCommand(file.OpenReadStream(), file.FileName));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to upload");
            }
        }
    }
}
