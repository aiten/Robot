namespace InputWpf.Views;

using System.Windows;
using System.Windows.Input;

using InputWpf.ViewModels;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel vm)
    {
        vm.Controller = new WindowNavigator(this);

        InitializeComponent();

        DataContext = vm;

        Loaded += async (v, e) =>
        {
            await (DataContext as MainWindowViewModel)!
                .LoadDataAsync();
        };
    }

    private void DockPanel_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        var dir = e.Key switch
        {
            Key.Left     => "270",
            Key.Right    => "90",
            Key.Up       => "0",
            Key.Down     => "180",
            Key.PageUp   => "45",
            Key.PageDown => "135",
            Key.End      => "225",
            Key.Home     => "315",
            Key.NumPad4  => "270",
            Key.NumPad6  => "90",
            Key.NumPad8  => "0",
            Key.NumPad2  => "180",
            Key.NumPad9  => "45",
            Key.NumPad3  => "135",
            Key.NumPad1  => "225",
            Key.NumPad7  => "315",
            _            => ""
        };
        if (!string.IsNullOrEmpty(dir))
        {
            (DataContext as MainWindowViewModel)!.DriveCommand.Execute(dir);
            e.Handled = true;
        }
    }
}