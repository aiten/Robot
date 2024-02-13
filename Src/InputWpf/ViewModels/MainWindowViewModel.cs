namespace InputWpf.ViewModels;

using System.Threading.Tasks;
using System.Windows.Input;

using InputService.Abstraction;

using InputWpf.Tools;

using Microsoft.Extensions.Configuration;

public class MainWindowViewModel : BaseViewModel
{
    #region crt

    public MainWindowViewModel(IRobotControlService robotControlService, IConfiguration configuration)
    {
        _robotControlService = robotControlService;
        _configuration       = configuration;

        _robotControlService.ReceivedMessage += async (_, e) => { await ReceivedMessageAsync(e.Item1, e.Item2); };

        _robotControlService.AddSubscribeToStatAndCmd();
    }

    private          IRobotControlService _robotControlService = default!;
    private readonly IConfiguration       _configuration;

    #endregion

    #region Properties

    private uint? _moveCount;

    public uint? MoveCount
    {
        get => _moveCount;
        set => SetProperty(ref _moveCount, value);
    }

    private string _lastMoveResult = string.Empty;

    public string LastMoveResult
    {
        get => _lastMoveResult;
        set => SetProperty(ref _lastMoveResult, value);
    }

    private uint _defaultSpeed = 255;

    public uint DefaultSpeed
    {
        get => _defaultSpeed;
        set => SetProperty(ref _defaultSpeed, value);
    }

    private uint _defaultDuration = 250;

    public uint DefaultDuration
    {
        get => _defaultDuration;
        set => SetProperty(ref _defaultDuration, value);
    }

    private uint _fwBwDuration = 500;

    public uint FwBwDuration
    {
        get => _fwBwDuration;
        set => SetProperty(ref _fwBwDuration, value);
    }

    #endregion

    #region Commands

    public ICommand DriveCommand              => new RelayCommand(async (parameter) => await Forward(parameter));
    public ICommand NewCommand                => new RelayCommand(async (_) => await New());
    public ICommand SetDefaultSpeedCommand    => new RelayCommand(async (_) => await SetDefaultSpeed());
    public ICommand SetDefaultDurationCommand => new RelayCommand(async (_) => await SetDefaultDuration());

    #endregion

    #region Operations

    public async Task ReceivedMessageAsync(string topic, string payload)
    {
        LastMoveResult = $"{topic} = {payload}";
        await Task.CompletedTask;
    }

    public async Task New()
    {
        MoveCount = null;
        await Task.CompletedTask;
    }

    public async Task SetDefaultSpeed()
    {
        LastMoveResult = await _robotControlService.SetDefaultSpeed(_configuration["Robot"] ?? "Robot", DefaultSpeed);
    }

    public async Task SetDefaultDuration()
    {
        LastMoveResult = await _robotControlService.SetDefaultDuration(_configuration["Robot"] ?? "Robot", DefaultDuration);
    }

    public async Task Forward(object? obj)
    {
        if (obj is not null)
        {
            var  direction = uint.Parse((string)obj);
            bool isFwBw    = direction == 0 || direction == 180;
            uint duration  = isFwBw ? FwBwDuration : DefaultDuration;
            LastMoveResult = await _robotControlService.Drive(_configuration["Robot"] ?? "Robot", direction, duration: duration);
            MoveCount      = (MoveCount ?? 0) + 1;
        }
    }

    public async Task LoadDataAsync()
    {
        await Task.CompletedTask;
    }

    #endregion
}