namespace RaceServer.Abstraction;

using System.Collections.Generic;

using MqttService.Abstraction;

public interface IMappingService : IMqttService
{
    IEnumerable<(string from, string to)> GetMapping();

    void UpdateMapping(string from, string to);
    void RemoveMapping(string from);
}