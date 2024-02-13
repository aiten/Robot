namespace InputService.Abstraction;

using System;
using System.Threading.Tasks;

using MqttService.Abstraction;

public interface IRobotControlService : IMqttService
{
    event EventHandler<(string, string)>? ReceivedMessage;

    Task<string> Go(string robotName, uint direction, uint? speed = null, uint? duration = null);
    Task<string> Drive(string robotName, uint direction, uint? speed = null, uint? duration = null);
    Task<string> SetDefaultSpeed(string robotName, uint speed);
    Task<string> SetDefaultDuration(string robotName, uint duration);
}