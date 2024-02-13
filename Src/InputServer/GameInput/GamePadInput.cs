namespace InputServer.GameInput;

using System;
using System.Threading.Tasks;

using InputService.Abstraction;

using Microsoft.Extensions.Logging;

using SharpDX.DirectInput;

public class GamePadInput : BaseGameInput
{
    public GamePadInput(ILogger logger, IRobotControlService service) : base(logger, service)
    {
    }

    public Joystick Joystick { get; set; } = default!;

    private bool _speed0Sent;
    private long _tickCount;

    private const int SEND_INTERVAL = 100;

    public override async Task Poll()
    {
        if (_tickCount > Environment.TickCount64)
        {
            return;
        }

        _tickCount = Environment.TickCount64 + SEND_INTERVAL;

        Joystick.Poll();

        var datas = Joystick.GetBufferedData(); // needed to get GetCurrentState

        var joystickState = new JoystickState();
        Joystick.GetCurrentState(ref joystickState);

        var x = (joystickState.X / 128) - 256;
        var y = (joystickState.Y / 128) - 256;

        uint direction = (uint)(Math.Atan2(-x, y) / Math.PI * 180.0) + 180;
        uint speed     = (uint)Math.Min(255, (int)Math.Sqrt((double)x * x + (double)y * y));

        if (speed < 30)
        {
            speed = 0;
        }
        else
        {
            int map(int value, int fromLow, int fromHigh, int toLow, int toHigh)
            {
                return (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
            }

            speed = (uint)map((int)speed, 0, 255, 30, 255);
        }


        if (speed != 0 || _speed0Sent == false)
        {
            await PublishGo(direction, speed, SEND_INTERVAL + 50);
            _speed0Sent = speed == 0;
        }
    }
    public override async Task Close()
    {
        await Task.CompletedTask;
        Joystick.Unacquire();
    }

}