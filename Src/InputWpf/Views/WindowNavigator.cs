namespace InputWpf.Views;

using System.Windows;

using InputWpf.Tools;

public class WindowNavigator : IWindowNavigator
{
    private readonly Window _window;

    public WindowNavigator(Window window)
    {
        _window = window;
    }

    public void CloseWindow()
    {
        _window.DialogResult = true;
        _window.Close();
    }
}