namespace RaceServer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using MqttService;
using MqttService.Abstraction;

using RaceServer.Abstraction;

public class MappingService : MqttService, IMappingService
{
    private readonly ILogger<MappingService> _logger;
    private readonly IConfiguration          _configuration;

    public event EventHandler<(string, string)>? PublishMessage;

    private readonly IDictionary<string, string> _mapping;
    private readonly IDictionary<string, string> _reverseMapping;

    public MappingService(ILogger<MappingService> logger, IConfiguration configuration)
    {
        _logger        = logger;
        _configuration = configuration;

        var mapping = _configuration.GetSection("RobotMapping").Get<IList<IList<string>>>()!;

        _mapping        = mapping.ToDictionary(m => m.First(),         m => m.Skip(1).First());
        _reverseMapping = mapping.ToDictionary(m => m.Skip(1).First(), m => m.First());

        AddSubscribeToStatAndCmd();
    }

    public async Task InitAsync()
    {
        await Task.CompletedTask;

        foreach (var name in _mapping.Keys)
        {
            PublishMessage?.Invoke(this, ($"{MqttConst.TOPIC_DISCOVERY}/{name}/config", "virtual"));
        }
    }

    public async Task OnMessageReceived(string topic, string payload)
    {
        await Task.CompletedTask;

        var fromInfo = AnalyseTopic(MqttConst.TOPIC_CMND, topic);

        if (fromInfo is not null)
        {
            if (_mapping.TryGetValue(fromInfo.Value.name, out var toName))
            {
                // forward message
                var forwardTopic = $"{MqttConst.TOPIC_CMND}/{toName}/{fromInfo.Value.tag}";
                PublishMessage?.Invoke(this, (forwardTopic, payload));
                _logger.LogInformation($"MappingService(OnMessageReceived): Map-Forward {topic} => {forwardTopic}");
            }
        }

        fromInfo = AnalyseTopic(MqttConst.TOPIC_STAT, topic);

        if (fromInfo is not null)
        {
            if (_reverseMapping.TryGetValue(fromInfo.Value.name, out var toName))
            {
                // forward message
                var forwardTopic = $"{MqttConst.TOPIC_STAT}/{toName}/{fromInfo.Value.tag}";
                PublishMessage?.Invoke(this, (forwardTopic, payload));
                _logger.LogInformation($"MappingService(OnMessageReceived): Map-Reverse {topic} => {forwardTopic}");
            }
        }
    }
}