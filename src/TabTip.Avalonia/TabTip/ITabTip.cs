namespace TabTip.Avalonia.TabTip;

public interface ITabTip
{
    IHardwareKeyboard Keyboard { get; }
    void Toggle(IntPtr hwnd);
}