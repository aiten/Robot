namespace InputServer.GameInput;

using System;
using System.Threading.Tasks;

using InputService.Abstraction;

using Microsoft.Extensions.Logging;

using SharpDX.DirectInput;

public abstract class BaseGameInput
{
    private readonly ILogger              _logger;
    private readonly IRobotControlService _service;

    private JoystickState? _joystickState;
    public  Joystick       Joystick { get; set; } = default!;

    private long _tickCount       = 0;
    private long _tickCountButton = 0;

    public BaseGameInput(ILogger logger, IRobotControlService service)
    {
        _logger  = logger;
        _service = service;
    }

    public string SendToName { get; set; } = "Virtual1";

    public abstract Task Poll();

    public virtual async Task Close()
    {
        await Task.CompletedTask;
        Joystick.Unacquire();
    }

    public JoystickState PollJoystick()
    {
        Joystick.Poll();

        var updates = Joystick.GetBufferedData();

        if (_joystickState == null)
        {
            _joystickState = new JoystickState();
            Joystick.GetCurrentState(ref _joystickState);
        }
        else
        {
            if (true)       // if usb has the same guid, we do not get all updates!
            {
                Joystick.GetCurrentState(ref _joystickState);
            }
            else
            {
                foreach (var data in updates)
                {
                    _joystickState.Update(data);
                }
            }
        }

        return _joystickState;
    }

    protected bool CheckButtonInterval(int interval)
    {
        if (_tickCountButton > Environment.TickCount64)
        {
            return false;
        }

        _tickCountButton = Environment.TickCount64 + interval;
        return true;
    }

    protected bool CheckSendInterval(int interval)
    {
        if (_tickCount > Environment.TickCount64)
        {
            return false;
        }

        _tickCount = Environment.TickCount64 + interval;
        return true;
    }

    private bool _speed0Sent;

    protected async Task LimitPublishGo(uint direction, uint speed, uint duration)
    {
        if (speed < 30)
        {
            speed = 0;
        }
        else
        {
            speed = (uint)Map((int)speed, 0, 255, 30, 255);
        }


        if (speed != 0 || _speed0Sent == false)
        {
            await PublishGo(direction, speed, duration);
            _speed0Sent = speed == 0;
        }
    }

    protected async Task PublishGo(uint direction, uint speed, uint duration)
    {
        await _service.Go(SendToName, direction, speed, duration);
        _logger.LogInformation($"{SendToName}: {direction}=>{speed}");
    }

    protected static int Map(int value, int fromLow, int fromHigh, int toLow, int toHigh)
    {
        return (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
    }
}