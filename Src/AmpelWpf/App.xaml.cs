namespace AmpelWpf;

using System.Windows;

using AmpelWpf.Views;

using InputService;
using InputService.Abstraction;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MqttService;
using MqttService.Abstraction;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private void AppStartup(object sender, StartupEventArgs e)
    {
        var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
                services
                    .AddHostedService<MqttBackgroundService>()
                    .AddSingleton<RobotControlService>()
                    .AddSingleton<IRobotControlService>(x => x.GetRequiredService<RobotControlService>()) // because of GameDeviceBackgroundService
                    .AddSingleton<IMqttService>(x => x.GetRequiredService<RobotControlService>())         // because of MqttBackgroundService
                    .AddAssemblyByName(
                        n => n.EndsWith("ViewModel"),
                        ServiceLifetime.Transient,
                        typeof(ViewModels.MainWindowViewModel).Assembly)
                    .AddTransient<MainWindow>());

        var host = hostBuilder.Build();

        AppService.ServiceProvider = host.Services;

        host.StartAsync();

        MainWindow = AppService.GetRequiredService<MainWindow>();
        MainWindow.ShowDialog();

        host.StopAsync();
    }
}