using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace TabTip.Avalonia.TabTip;

[SupportedOSPlatform("windows")]
public partial class WindowsSessionInfo : ISessionInfo
{
    public bool IsRemoteSession() => GetSystemMetrics(SM_REMOTESESSION) != 0;

    private const int SM_REMOTESESSION = 0x1000;

    [LibraryImport("user32.dll")]
    private static partial int GetSystemMetrics(int nIndex);
}
