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

        _sendTo = _configuration["Robot"] ?? "Virtual1";
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

    private uint _speed = 150;

    public uint Speed
    {
        get => _speed;
        set => SetProperty(ref _speed, value);
    }

    private uint _duration = 250;

    public uint Duration
    {
        get => _duration;
        set => SetProperty(ref _duration, value);
    }

    private uint _fwBwDuration = 500;

    public uint FwBwDuration
    {
        get => _fwBwDuration;
        set => SetProperty(ref _fwBwDuration, value);
    }

    private string _sendTo;

    public string SendTo
    {
        get => _sendTo;
        set => SetProperty(ref _sendTo, value);
    }

    #endregion

    #region Commands

    public ICommand DriveCommand  => new RelayCommand(async (parameter) => await Drive(parameter));
    public ICommand OptionCommand => new RelayCommand(async (parameter) => await Option(parameter));
    public ICommand NewCommand    => new RelayCommand(async (_) => await New());

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

    public async Task Drive(object? obj)
    {
        if (obj is not null)
        {
            var  direction = uint.Parse((string)obj);
            bool isFwBw    = direction == 0 || direction == 180;
            uint duration  = isFwBw ? FwBwDuration : Duration;
            LastMoveResult = await _robotControlService.Drive(SendTo, direction, Speed, duration);
            MoveCount      = (MoveCount ?? 0) + 1;
        }
    }

    public async Task Option(object? obj)
    {
        if (obj is not null)
        {
            var option = uint.Parse((string)obj);
            switch (option)
            {
                case 0:
                    LastMoveResult = await _robotControlService.Ping(SendTo);
                    break;
            }
        }
    }

    public async Task LoadDataAsync()
    {
        await Task.CompletedTask;
    }

    #endregion
}