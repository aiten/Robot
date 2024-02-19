namespace AmpelWpf.Views;

using System.Windows;

using AmpelWpf.ViewModels;

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
}