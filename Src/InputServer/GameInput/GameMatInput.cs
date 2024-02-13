namespace InputServer.GameInput;

using System;
using System.Threading.Tasks;

using InputService.Abstraction;

using Microsoft.Extensions.Logging;

using SharpDX.DirectInput;

public class GameMatInput : BaseGameInput
{
    private readonly ILogger _logger;

    public GameMatInput(ILogger logger, IRobotControlService service) : base(logger, service)
    {
        _logger = logger;
    }

    public Joystick Joystick { get; set; } = default!;

    private bool _speed0Sent;
    private long _tickCount;
    private long _tickCountSend;

    uint   _speedIdx        = 3;
    uint[] _speedTable      = new uint[] { 75, 100, 150, 200, 255 };
    bool   _incSpeedPressed = false;
    bool   _decSpeedPressed = false;

    private const int BUTTON_INTERVAL = 10;
    private const int SEND_INTERVAL   = 100;

    public override async Task Poll()
    {
        if (_tickCount > Environment.TickCount64)
        {
            return;
        }

        _tickCount = Environment.TickCount64 + BUTTON_INTERVAL;

        Joystick.Poll();

        var datas = Joystick.GetBufferedData(); // needed to get GetCurrentState

        var joystickState = new JoystickState();
        Joystick.GetCurrentState(ref joystickState);

        uint direction = 0;
        uint speed     = 0;


        if (joystickState.Buttons[9])
        {
            if (!_incSpeedPressed)
            {
                if (_speedIdx < _speedTable.Length - 1)
                {
                    _speedIdx++;
                    _logger.LogInformation($"new speed: {_speedTable[_speedIdx]}");
                }

                _incSpeedPressed = true;
            }
        }
        else
        {
            _incSpeedPressed = false;
        }

        if (joystickState.Buttons[8])
        {
            if (!_decSpeedPressed)
            {
                if (_speedIdx > 0)
                {
                    _speedIdx--;
                    _logger.LogInformation($"new speed: {_speedTable[_speedIdx]}");
                }

                _decSpeedPressed = true;
            }
        }
        else
        {
            _decSpeedPressed = false;
        }

        if (_tickCountSend > Environment.TickCount64)
        {
            return;
        }

        _tickCountSend = Environment.TickCount64 + SEND_INTERVAL;


        var buttonDirections = new uint[] { 270, 180, 0, 90, 135, 225, 315, 45 };

        for (var buttonIdx = 0; buttonIdx < buttonDirections.Length; buttonIdx++)
        {
            if (joystickState.Buttons[buttonIdx])
            {
                direction = buttonDirections[buttonIdx];
                speed     = _speedTable[_speedIdx];
            }
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