namespace TabTip.Avalonia.TabTip;

public interface ITabTip
{
    IHardwareKeyboard Keyboard { get; }
    ISessionInfo Session { get; }
    void Toggle(IntPtr hwnd);
}
