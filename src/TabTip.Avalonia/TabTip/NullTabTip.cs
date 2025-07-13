namespace TabTip.Avalonia.TabTip;

public class NullTabTip : ITabTip
{
    public void Toggle(IntPtr hwnd)
    {
        // We do nothing
    }
}