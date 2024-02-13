using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MqttService;
using MqttService.Abstraction;

using RaceServer;


var hostBuilder = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
        services
            .AddHostedService<MqttBackgroundService>()
            .AddSingleton<IMqttService, MappingService>());

var host = hostBuilder.Build();

await host.RunAsync();