namespace MqttService.Abstraction;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IMqttService
{
    event EventHandler<(string, string)>? PublishMessage;

    ICollection<string> SubscribeTo { get; }

    void AddSubscribeToStatAndCmd();

    Task OnMessageReceived(string topic, string payload);

    Task InitAsync();

}