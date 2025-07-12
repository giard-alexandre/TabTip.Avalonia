namespace TabTip.Avalonia;

public class NullTabTip : ITabTip
{
    public void Toggle(IntPtr hwnd)
    {
        // We do nothing
    }
}