namespace InputServer.GameInput;

using InputService.Abstraction;

using Microsoft.Extensions.Logging;

using SharpDX.DirectInput;

internal class GameInputFactory
{
    public enum DeviceType
    {
        None = 0,
        GamePad,
        GameMat,
        Wheel
    }

    public static BaseGameInput? Create(DeviceType type, Joystick joystick, ILogger logger, IRobotControlService service, string name)
    {
        return type switch
        {
            DeviceType.GameMat => new GameMatInput(logger, service) { Joystick = joystick, SendToName = name },
            DeviceType.GamePad => new GamePadInput(logger, service) { Joystick = joystick, SendToName = name },
            DeviceType.Wheel   => new WheelInput(logger, service) { Joystick   = joystick, SendToName = name },
            _                  => null
        };
    }
}