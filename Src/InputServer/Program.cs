using InputServer;

using InputService;
using InputService.Abstraction;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MqttService;
using MqttService.Abstraction;


var hostBuilder = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
            services
                .AddHostedService<MqttBackgroundService>()
                .AddHostedService<GameDeviceBackgroundService>()
                .AddSingleton<RobotControlService>()
                .AddSingleton<IRobotControlService>(x => x.GetRequiredService<RobotControlService>()) // because of GameDeviceBackgroundService
                .AddSingleton<IMqttService>(x => x.GetRequiredService<RobotControlService>())         // because of MqttBackgroundService
    );

var host = hostBuilder.Build();

await host.RunAsync();