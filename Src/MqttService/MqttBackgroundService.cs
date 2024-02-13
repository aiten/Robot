namespace MqttService;

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using global::MqttService.Abstraction;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using MQTTnet;
using MQTTnet.Client;

public class MqttBackgroundService : BackgroundService
{
    private readonly ILogger<MqttBackgroundService> _logger;
    private readonly IConfiguration                 _configuration;
    private readonly IMqttService                   _mqttService;

    private IMqttClient?       _mqttClient;
    private MqttClientOptions? _mqttClientOptions;
    private CancellationToken  _cancellationToken = CancellationToken.None;

    public MqttBackgroundService(ILogger<MqttBackgroundService> logger, IConfiguration configuration, IMqttService mqttService)
    {
        _logger        = logger;
        _configuration = configuration;
        _mqttService   = mqttService;

        _mqttService.PublishMessage += async (_, e) => { await PublishAsync(e.Item1, e.Item2, false); };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _cancellationToken = stoppingToken;

        _logger.LogInformation("MqttService(ExecuteAsync):started");

        await InitAsync();

        int round = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(100, stoppingToken);
            round++;
            if (round >= 100)
            {
                _logger.LogInformation($"MqttService(MqttService): 10 seconds have passed");
                round = 0;
            }
        }
    }

    private readonly string ClientId = $"MqttService_{Guid.NewGuid()}";

    private async Task InitAsync()
    {
        var mqtt = _configuration.GetSection("Mqtt").Get<MqttConfig>()!;

        var factory = new MqttFactory();

        _mqttClient        = factory.CreateMqttClient();
        _mqttClientOptions = CreateMqttClientOptions(mqtt);

        _mqttClient.ConnectedAsync                  += (async e => { await OnConnectAsync(); });
        _mqttClient.ApplicationMessageReceivedAsync += (async mqttEvent => { await OnApplicationMessageReceiveAsync(mqttEvent); });
        _mqttClient.DisconnectedAsync               += (async e => { await OnDisconnectAsync(e); });

        try
        {
            var result = await _mqttClient.ConnectAsync(_mqttClientOptions, _cancellationToken);
            if (result.ResultCode != MqttClientConnectResultCode.Success)
            {
                _logger.LogError("MqttService(InitAsync):ConnectAsync failed with errorCode: {MqttResultcode}", result.ResultCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MqttService(InitAsync):ConnectAsync");
        }

        _logger.LogInformation("MqttService(InitAsync): connected with MqttClientId: {MqttClientId} ", ClientId);

        await _mqttService.InitAsync();
    }

    private MqttClientOptions CreateMqttClientOptions(MqttConfig mqtt)
    {
        return new MqttClientOptionsBuilder()
            .WithClientId(ClientId)
            .WithTcpServer(mqtt.Broker, mqtt.Port)
            .WithCredentials(mqtt.User, mqtt.Password)
            .WithTlsOptions(configure =>
            {
                configure
                    .WithAllowUntrustedCertificates()
                    .WithIgnoreCertificateChainErrors(true)
                    .WithIgnoreCertificateRevocationErrors(true)
                    .WithSslProtocols(System.Security.Authentication.SslProtocols.Tls12)
                    .UseTls(false);
            })
            .WithCleanSession()
            //.WithCommunicationTimeout(TimeSpan.FromSeconds(2))
            .Build();
    }

    private async Task OnConnectAsync()
    {
        _logger.LogInformation("MqttService(OnConnectAsync): Mqtt connected");

        foreach (var topic in _mqttService.SubscribeTo)
        {
            await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build(), _cancellationToken);
            _logger.LogInformation($"MqttService(OnConnectAsync): to topic {topic} subscribed");
        }
    }

    private async Task OnApplicationMessageReceiveAsync(MqttApplicationMessageReceivedEventArgs mqttEvent)
    {
        string topic   = mqttEvent.ApplicationMessage.Topic;
        string payload = string.Empty;

        if (mqttEvent.ApplicationMessage.PayloadSegment.Array != null)
        {
            payload = Encoding.UTF8.GetString(mqttEvent.ApplicationMessage.PayloadSegment.ToArray());
        }

        var qos      = mqttEvent.ApplicationMessage.QualityOfServiceLevel;
        var retained = mqttEvent.ApplicationMessage.Retain;

        _logger.LogInformation($"Mqtt {topic};{payload};{retained}");


        await _mqttService.OnMessageReceived(topic, payload);

        await Task.CompletedTask;
    }

    private async Task OnDisconnectAsync(MqttClientDisconnectedEventArgs e)
    {
        _logger.LogError("MqttService(OnDisconnectAsync): Disconnected with reason: {Reasoncode}", e.Reason);
        // Reconnect
        await _mqttClient!.ConnectAsync(_mqttClientOptions, _cancellationToken);
    }

    public async Task<bool> PublishAsync(string topic, string value, bool retain)
    {
        if (_mqttClient == null || _mqttClient.IsConnected == false)
        {
            _logger.LogError("MqttService(Publish): MqttClient not connected");
            return false;
        }

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(value)
            .WithRetainFlag(retain)
            .Build();

        await _mqttClient.PublishAsync(message, _cancellationToken);

        return true;
    }
}