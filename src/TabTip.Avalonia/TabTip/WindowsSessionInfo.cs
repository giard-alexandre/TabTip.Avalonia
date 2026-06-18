using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace TabTip.Avalonia.TabTip;

[SupportedOSPlatform("windows")]
public class WindowsSessionInfo : ISessionInfo
{
    public bool IsRemoteSession() => GetSystemMetrics(SM_REMOTESESSION) != 0;

    private const int SM_REMOTESESSION = 0x1000;

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);
}
