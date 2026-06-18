namespace TabTip.Avalonia.TabTip;

public class NullTabTip : ITabTip
{
    public IHardwareKeyboard Keyboard { get; } = new NullKeyboard();
    public ISessionInfo Session { get; } = new NullSessionInfo();

    public void Toggle(IntPtr hwnd)
    {
        // We do nothing
    }
}

public class NullKeyboard : IHardwareKeyboard
{
    // Conservative default for platforms where we can't actually probe: report a physical
    // keyboard so a KeyboardDetectionTriggerPolicy with the default suppression set keeps
    // the on-screen keyboard closed instead of popping it on every focus.
    public HardwareKeyboardType GetConnected() => HardwareKeyboardType.Physical;
}

public class NullSessionInfo : ISessionInfo
{
    public bool IsRemoteSession() => false;
}