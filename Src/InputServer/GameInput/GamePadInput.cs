namespace InputServer.GameInput;

using System;
using System.Threading.Tasks;

using InputService.Abstraction;

using Microsoft.Extensions.Logging;

public class GamePadInput : BaseGameInput
{
    public GamePadInput(ILogger logger, IRobotControlService service) : base(logger, service)
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

        var x = Map(joystickState.X, 0, 65535, -255, 255);
        var y = Map(joystickState.Y, 0, 65535, -255, 255);

        var direction = (uint)(Math.Atan2(-x, y) / Math.PI * 180.0) + 180;
        var speed     = (uint)Math.Min(255, (int)Math.Sqrt((double)x * x + (double)y * y));

        await LimitPublishGo(direction, speed, SEND_INTERVAL + 50);
    }
}