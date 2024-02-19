namespace InputServer.GameInput.Tool;

using Microsoft.Extensions.Logging;

using SharpDX.DirectInput;

public class Gear
{
    private readonly ILogger _logger;
    private readonly int     _incButtonIdx;
    private readonly int     _decButtonIdx;

    uint   _speedIdx   = 3;
    uint[] _speedTable = new uint[] { 75, 100, 150, 200, 255 };

    private ButtonState _incButtonState = new ButtonState();
    private ButtonState _decButtonState = new ButtonState();

    public Gear(ILogger logger, int incButtonIdx, int decButtonIdx)
    {
        _logger       = logger;
        _incButtonIdx = incButtonIdx;
        _decButtonIdx = decButtonIdx;
    }

    public uint Poll(JoystickState joystickState)
    {
        if (_incButtonState.IsPressed(joystickState.Buttons[9]))
        {
            if (_speedIdx < _speedTable.Length - 1)
            {
                _speedIdx++;
                _logger.LogInformation($"new speed: {_speedTable[_speedIdx]}");
            }
        }

        if (_decButtonState.IsPressed(joystickState.Buttons[8]))
        {
            if (_speedIdx > 0)
            {
                _speedIdx--;
                _logger.LogInformation($"new speed: {_speedTable[_speedIdx]}");
            }
        }

        return _speedTable[_speedIdx];
    }
}