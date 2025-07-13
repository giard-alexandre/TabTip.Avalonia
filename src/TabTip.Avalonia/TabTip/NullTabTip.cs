namespace TabTip.Avalonia.TabTip;

public class NullTabTip : ITabTip
{
    public IHardwareKeyboard Keyboard { get; } = new NullKeyboard();

    public void Toggle(IntPtr hwnd)
    {
        // We do nothing
    }
}

public class NullKeyboard : IHardwareKeyboard
{
    public bool IsHardwareKeyboardConnected() => true;
}