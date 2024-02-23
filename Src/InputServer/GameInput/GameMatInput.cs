namespace InputServer.GameInput;

using System.Threading.Tasks;

using InputServer.GameInput.Tool;

using InputService.Abstraction;

using Microsoft.Extensions.Logging;

public class GameMatInput : BaseGameInput
{
    private readonly ILogger _logger;

    private const int IncButtonIdx = 9;
    private const int DecButtonIdx = 9;

    public GameMatInput(ILogger logger, IRobotControlService service) : base(logger, service)
    {
        _logger = logger;
        _gear   = new Gear(_logger, IncButtonIdx, DecButtonIdx);
    }

    private const int BUTTON_INTERVAL = 10;
    private const int SEND_INTERVAL   = 100;

    private readonly Gear _gear;

    public override async Task Poll()
    {
        if (!CheckButtonInterval(BUTTON_INTERVAL))
        {
            return;
        }

        var joystickState = PollJoystick();

        uint gearSpeed = _gear.Poll(joystickState);

        if (!CheckSendInterval(SEND_INTERVAL))
        {
            return;
        }

        uint direction = 0;
        uint speed     = 0;

        var buttonDirections = new uint[] { 270, 180, 0, 90, 135, 225, 315, 45 };

        for (var buttonIdx = 0; buttonIdx < buttonDirections.Length; buttonIdx++)
        {
            if (joystickState.Buttons[buttonIdx])
            {
                direction = buttonDirections[buttonIdx];
                speed     = gearSpeed;
            }
        }

        await LimitPublishGo(direction, speed, SEND_INTERVAL + SEND_INTERVAL_ADD);
    }
}