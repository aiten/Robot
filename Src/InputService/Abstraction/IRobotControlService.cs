namespace InputService.Abstraction;

using System;
using System.Threading.Tasks;

using MqttService.Abstraction;

public interface IRobotControlService : IMqttService
{
    event EventHandler<(string, string)>? ReceivedMessage;

    Task<string> Go(string    robotName, uint direction, uint? speed = null, uint? duration = null);
    Task<string> Drive(string robotName, uint direction, uint? speed = null, uint? duration = null);
    Task<string> Stop(string robotName);

    Task<string> ToRed(string   robotName, uint idx, uint? stayOnGreen);
    Task<string> ToGreen(string robotName, uint idx, uint? stayOnGreen);
}