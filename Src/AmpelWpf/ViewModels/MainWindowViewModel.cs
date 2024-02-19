namespace AmpelWpf.ViewModels;

using System.Drawing;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

using AmpelWpf.Tools;

using InputService.Abstraction;

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

    private string _lastResult = string.Empty;

    public string LastResult
    {
        get => _lastResult;
        set => SetProperty(ref _lastResult, value);
    }

    private uint _delay = 1000;

    public uint Delay
    {
        get => _delay;
        set => SetProperty(ref _delay, value);
    }

    private string _ampelColor0 = nameof(Color.Red);

    public string AmpelColor0
    {
        get => _ampelColor0;
        set => SetProperty(ref _ampelColor0, value);
    }

    private string _ampelColor1 = nameof(Color.Red);

    public string AmpelColor1
    {
        get => _ampelColor1;
        set => SetProperty(ref _ampelColor1, value);
    }

    private string _robotName = "Robot1";

    public string RobotName
    {
        get => _robotName;
        set => SetProperty(ref _robotName, value);
    }

    #endregion

    #region Commands

    public ICommand ToRedCommand   => new RelayCommand(async (parameter) => await ToRed(parameter));
    public ICommand ToGreenCommand => new RelayCommand(async (parameter) => await ToGreen(parameter));

    #endregion

    #region Operations

    public async Task ReceivedMessageAsync(string topic, string payload)
    {
        LastResult = $"{topic} = {payload}";

        var regEx = new Regex("robot/stat/(.*)/phase(.*)");
        var match = regEx.Match(topic);

        if (match.Success && match.Groups[1].Value == RobotName)
        {
            var idx = match.Groups[2].Value;

            var jsonObject = JsonNode.Parse(payload)!.AsObject();

            var phaseJson = jsonObject["phase"];
            if (phaseJson is not null)
            {
                var phase = (int)phaseJson;
                var color = phase switch
                {
                    0  => nameof(Color.Red),
                    1  => nameof(Color.Orange),
                    2  => nameof(Color.Green),
                    3  => nameof(Color.Gray),
                    4  => nameof(Color.Green),
                    5  => nameof(Color.Gray),
                    6  => nameof(Color.Green),
                    7  => nameof(Color.Gray),
                    8  => nameof(Color.Green),
                    9  => nameof(Color.Gray),
                    10 => nameof(Color.Green),
                    11 => nameof(Color.Orange),
                    _  => nameof(Color.Gray),
                };

                switch (idx)
                {
                    case "0":
                        AmpelColor0 = color;
                        break;
                    case "1":
                        AmpelColor1 = color;
                        break;
                }

                return;
            }
        }

        await Task.CompletedTask;
    }

    public async Task ToRed(object? obj)
    {
        if (obj is not null)
        {
            var idx = uint.Parse((string)obj);
            LastResult = await _robotControlService.ToRed(RobotName, idx, Delay);
        }
    }

    public async Task ToGreen(object? obj)
    {
        if (obj is not null)
        {
            var idx = uint.Parse((string)obj);
            LastResult = await _robotControlService.ToGreen(RobotName, idx, Delay);
        }
    }

    public async Task LoadDataAsync()
    {
        RobotName = _configuration["Robot"] ?? "Robot";
        await Task.CompletedTask;
    }

    #endregion
}