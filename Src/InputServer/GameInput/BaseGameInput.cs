namespace InputServer.GameInput;

using System.Threading.Tasks;

using InputService.Abstraction;

using Microsoft.Extensions.Logging;

public abstract class BaseGameInput
{
    private readonly ILogger              _logger;
    private readonly IRobotControlService _service;

    public BaseGameInput(ILogger logger, IRobotControlService service)
    {
        _logger  = logger;
        _service = service;
    }

    public string SendToName { get; set; } = "Virtual1";

    public abstract Task Poll();

    public abstract Task Close();

    public async Task PublishGo(uint direction, uint speed, uint duration)
    {
        await _service.Go(SendToName, direction, speed, duration);
        _logger.LogInformation($"{SendToName}: {direction}=>{speed}");
    }
}