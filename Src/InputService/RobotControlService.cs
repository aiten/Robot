namespace InputService;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using InputService.Abstraction;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using MqttService;
using MqttService.Abstraction;

public class RobotControlService : MqttService, IRobotControlService
{
    private readonly ILogger<RobotControlService> _logger;
    private readonly IConfiguration _configuration;

    public event EventHandler<(string, string)>? PublishMessage = default;
    public event EventHandler<(string, string)>? ReceivedMessage = default;


    public RobotControlService(ILogger<RobotControlService> logger, IConfiguration configuration)
    {
        _logger = logger;
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

    public async Task<string> Drive(string robotName, uint direction, uint? speed, uint? duration)
    {
        try
        {
            PublishMessage?.Invoke(this, ($"{MqttConst.TOPIC_CMND}/{robotName}/drive", $"{{ \"direction\": {direction} }}"));
            await Task.CompletedTask;
            return string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
    public async Task<string> Go(string robotName, uint direction, uint? speed, uint? duration)
    {
        var speedStr = speed.HasValue ? $"\"speed\": {speed.Value}," : string.Empty;
        var durationStr = duration.HasValue ? $"\"duration\": {duration.Value}," : string.Empty;

        try
        {
            PublishMessage?.Invoke(this, ($"{MqttConst.TOPIC_CMND}/{robotName}/go", $"{{ {speedStr}{durationStr}\"direction\": {direction} }}"));
            await Task.CompletedTask;
            return string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public async Task<string> SetDefaultSpeed(string robotName, uint speed)
    {
        try
        {
            await Task.CompletedTask;
            PublishMessage?.Invoke(this, ($"{MqttConst.TOPIC_CMND}/{robotName}/speed", $"{speed}"));
            return string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public async Task<string> SetDefaultDuration(string robotName, uint duration)
    {
        try
        {
            await Task.CompletedTask;
            PublishMessage?.Invoke(this, ($"{MqttConst.TOPIC_CMND}/{robotName}/duration", $"{duration}"));
            return string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
}