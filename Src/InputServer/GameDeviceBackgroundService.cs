namespace InputServer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using InputServer.GameInput;

using InputService.Abstraction;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SharpDX.DirectInput;

public class GameDeviceBackgroundService : BackgroundService
{
    private readonly ILogger<GameDeviceBackgroundService> _logger;
    private readonly IConfiguration                       _configuration;
    private readonly IRobotControlService                 _service;
    private          CancellationToken                    _cancellationToken = CancellationToken.None;

    public GameDeviceBackgroundService(ILogger<GameDeviceBackgroundService> logger, IConfiguration configuration, IRobotControlService service)
    {
        _logger        = logger;
        _configuration = configuration;
        _service       = service;
    }

    private const int SEND_INTERVAL = 100;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _cancellationToken = stoppingToken;

        _logger.LogInformation("GameDeviceBackgroundService(ExecuteAsync):started");

        var devices    = await EnumInputDevices();
        var gameInputs = await CreateGameInput(devices);

        if (!gameInputs.Any())
        {
            _logger.LogInformation("GameDeviceBackgroundService(ExecuteAsync):not device found");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(5, stoppingToken);

            foreach (var gameInput in gameInputs)
            {
                await gameInput.Poll();
            }
        }

        foreach (var gameInput in gameInputs)
        {
            await gameInput.Close();
        }
    }

    private async Task<IList<Joystick>> EnumInputDevices()
    {
        await Task.CompletedTask;

        var devices = new List<Joystick>();

        var directInput = new DirectInput();

        var all = directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices);

        foreach (var deviceInstance in all)
        {
            var joystickGuid = deviceInstance.InstanceGuid;
            devices.Add(new Joystick(directInput, joystickGuid));
        }

        foreach (var deviceInstance in directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
        {
            var joystickGuid = deviceInstance.InstanceGuid;
            devices.Add(new Joystick(directInput, joystickGuid));
        }

        return devices;
    }

    private async Task<IList<BaseGameInput>> CreateGameInput(IList<Joystick> devices)
    {
        var gameInputs = new List<BaseGameInput>();

        foreach (var device in devices)
        {
            var gameInput = await CreateGameInput(device);
            if (gameInput is not null)
            {
                gameInputs.Add(gameInput);
            }
        }

        return gameInputs;
    }

    private int _count = 0;

    private async Task<BaseGameInput?> CreateGameInput(Joystick device)
    {
        await Task.CompletedTask;

        var deviceConfigInfo = _configuration.GetSection("Devices").Get<IList<DeviceInfo>>();

        _count++;
        var type   = GameInputFactory.DeviceType.None;
        var sendTo = $"Virtual{_count}";

        _logger.LogInformation($"Found Joystick/Gamepad with GUID: {device.Information.InstanceGuid} - {device.Information.InstanceName}");
        _logger.LogInformation($"Effect available: {string.Join(',', device.GetEffects().Select(effectInfo => effectInfo.Name))}");

        var info = (deviceConfigInfo?.FirstOrDefault(d => d.Guid == device.Information.InstanceGuid)) ??
                   deviceConfigInfo?.FirstOrDefault(d => d.Name == device.Information.InstanceName);

        if (info is null)
        {
            if (device.Capabilities.ButtonCount == 10)
            {
                type = GameInputFactory.DeviceType.GameMat;
                info = deviceConfigInfo?.FirstOrDefault(d => d.Type == "GameMat");
                sendTo = info?.SendTo ?? sendTo;
            }
            else if (device.Capabilities.AxeCount == 5)
            {
                type   = GameInputFactory.DeviceType.GamePad;
                info   = deviceConfigInfo?.FirstOrDefault(d => d.Type == "GamePad");
                sendTo = info?.SendTo ?? sendTo;
            }
        }
        else
        {
            type   = Enum.Parse<GameInputFactory.DeviceType>(info.Type);
            sendTo = info.SendTo;
        }

        device.Properties.BufferSize = 128;
        device.Acquire();

        return GameInputFactory.Create(type, device, _logger, _service, sendTo);
    }
}