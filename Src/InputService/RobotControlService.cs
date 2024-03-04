namespace InputService;

using System;
using System.Threading.Tasks;

using InputService.Abstraction;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using MqttService;
using MqttService.Abstraction;

public class RobotControlService : MqttService, IRobotControlService
{
    private readonly ILogger<RobotControlService> _logger;
    private readonly IConfiguration               _configuration;

    public event EventHandler<(string, string)>? PublishMessage  = default;
    public event EventHandler<(string, string)>? ReceivedMessage = default;


    public RobotControlService(ILogger<RobotControlService> logger, IConfiguration configuration)
    {
        _logger        = logger;
        _configuration = configuration;
    }

    public async Task InitAsync()
    {
        await Task.CompletedTask;
    }

    public async Task OnMessageReceived(string topic, string payload)
    {
        await Task.CompletedTask;
        ReceivedMessage?.Invoke(this, (topic, payload));
    }

    public async Task<string> Ping(string robotName)
    {
        try
        {
            PublishMessage?.Invoke(this, ($"{MqttConst.TOPIC_CMND}/{robotName}/ping", string.Empty));
            await Task.CompletedTask;
            return string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    private async Task<string> DriveOrGo(string robotName, string command, uint direction, uint? speed, uint? duration)
    {
        var speedStr    = speed.HasValue ? $"\"speed\": {speed.Value}," : string.Empty;
        var durationStr = duration.HasValue ? $"\"duration\": {duration.Value}," : string.Empty;

        try
        {
            PublishMessage?.Invoke(this, ($"{MqttConst.TOPIC_CMND}/{robotName}/{command}", $"{{ {speedStr}{durationStr}\"direction\": {direction} }}"));
            await Task.CompletedTask;
            return string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public async Task<string> Drive(string robotName, uint direction, uint? speed, uint? duration)
    {
        return await DriveOrGo(robotName, "drive", direction, speed, duration);
    }

    public async Task<string> Go(string robotName, uint direction, uint? speed, uint? duration)
    {
        return await DriveOrGo(robotName, "go", direction, speed, duration);
    }

    public async Task<string> Stop(string robotName)
    {
        try
        {
            PublishMessage?.Invoke(this, ($"{MqttConst.TOPIC_CMND}/{robotName}/stop", string.Empty));
            await Task.CompletedTask;
            return string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public async Task<string> ToRed(string robotName, uint idx, uint? stayOnRed)
    {
        var durationStr = stayOnRed.HasValue ? $"\"delay\": {stayOnRed.Value}," : string.Empty;

        try
        {
            PublishMessage?.Invoke(this, ($"{MqttConst.TOPIC_CMND}/{robotName}/set", $"{{ {durationStr}\"idx\":{idx},\"toRed\": true }}"));
            await Task.CompletedTask;
            return string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public async Task<string> ToGreen(string robotName, uint idx, uint? stayOnGreen)
    {
        var durationStr = stayOnGreen.HasValue ? $"\"delay\": {stayOnGreen.Value}," : string.Empty;

        try
        {
            PublishMessage?.Invoke(this, ($"{MqttConst.TOPIC_CMND}/{robotName}/set", $"{{ {durationStr}\"idx\":{idx},\"toGreen\": true }}"));
            await Task.CompletedTask;
            return string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
}