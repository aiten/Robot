namespace InputServer.GameInput.Tool;

public class ButtonState
{
    private bool _wasPressed;

    public ButtonState()
    {
    }

    public bool IsPressed(bool value)
    {
        if (_wasPressed || !value)
        {
            _wasPressed = false;
            return false;
        }

        _wasPressed = true;
        return true;
    }
}