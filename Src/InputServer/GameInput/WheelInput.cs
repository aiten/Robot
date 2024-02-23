namespace InputServer.GameInput;

using System.Threading.Tasks;

using InputService.Abstraction;

using Microsoft.Extensions.Logging;

public class WheelInput : BaseGameInput
{
    public WheelInput(ILogger logger, IRobotControlService service) : base(logger, service)
    {
    }

    private const int SEND_INTERVAL = 100;

    public override async Task Poll()
    {
        if (!CheckSendInterval(SEND_INTERVAL))
        {
            return;
        }

        var joystickState = PollJoystick();

        var direction = (uint)Map(joystickState.X, 0, 65535, 360 - 90, 360 + 90) % 360;

        var speedFw = (uint)Map(joystickState.Y,         0, 65535, 255, 0);
        var speedBw = (uint)Map(joystickState.RotationZ, 0, 65535, 255, 0);

        var speed = speedFw;

        if (speedFw < 10 && speedBw > 10)
        {
            direction = (direction + 180) % 360;
            speed     = speedBw;
        }

        await LimitPublishGo(direction, speed, SEND_INTERVAL + SEND_INTERVAL_ADD);
    }
}