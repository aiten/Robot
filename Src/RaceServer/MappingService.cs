namespace RaceServer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MqttService;
using MqttService.Abstraction;

using RaceServer.Abstraction;

public class MappingService : MqttService, IMappingService
{
    private readonly ILogger<MappingService>       _logger;
    private readonly IOptionsMonitor<RobotMapping> _optionsMapping;

    public event EventHandler<(string, string)>? PublishMessage;

    private IDictionary<string, string> _mapping        = default!;
    private IDictionary<string, string> _reverseMapping = default!;

    public MappingService(ILogger<MappingService> logger, IOptionsMonitor<RobotMapping> optionsMapping)
    {
        _logger         = logger;
        _optionsMapping = optionsMapping;

        LoadMapping();

        AddSubscribeToStatAndCmd();

        optionsMapping.OnChange((_, _) => LoadMapping());
    }

    private void LoadMapping()
    {
        _logger.LogInformation("Read New mapping configuration");

        var mapping = _optionsMapping.CurrentValue.Map;

        _mapping        = mapping.ToDictionary(m => m.First(),         m => m.Skip(1).First());
        _reverseMapping = mapping.ToDictionary(m => m.Skip(1).First(), m => m.First());
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