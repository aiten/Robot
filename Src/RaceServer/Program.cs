using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MqttService;
using MqttService.Abstraction;

using RaceServer;


var hostBuilder = Host.CreateDefaultBuilder()
    //reloadOnChange: true is already done in base:  .ConfigureAppConfiguration((context, config) => { config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true); })
    .ConfigureServices(services =>
        services
            .AddHostedService<MqttBackgroundService>()
            .AddSingleton<IMqttService, MappingService>())
    .ConfigureServices((context, services) =>
    {
        var configurationRoot = context.Configuration;
        services.Configure<RobotMapping>(configurationRoot.GetSection(nameof(RobotMapping)));
    });

var host = hostBuilder.Build();

await host.RunAsync();