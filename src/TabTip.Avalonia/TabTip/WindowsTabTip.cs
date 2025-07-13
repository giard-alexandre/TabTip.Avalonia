using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace TabTip.Avalonia.TabTip;

[SupportedOSPlatform("windows")]
public class WindowsTabTip : ITabTip
{
    public IHardwareKeyboard Keyboard { get; } = new WindowsHardwareKeyboard();

    public void Toggle(IntPtr hwnd)
    {
        UIHostNoLaunch uiHostNoLaunch;
        try
        {
            uiHostNoLaunch = new UIHostNoLaunch();
        }
        catch(COMException e)
        {
            // The process was not started before, so we start it.
            if ((uint)e.HResult == 0x80040154)
            {
                Process p = new()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "tabtip.exe",
                        UseShellExecute = true
                    }
                };
                p.Start();
            }
            else
                throw;

            return;
        }

        // ReSharper disable once SuspiciousTypeConversion.Global
        var tipInvocation = (ITipInvocation)uiHostNoLaunch;
        tipInvocation.Toggle(hwnd);
        Marshal.ReleaseComObject(uiHostNoLaunch);
    }
    
    [ComImport, Guid("4ce576fa-83dc-4F88-951c-9d0782b4e376")]
    private class UIHostNoLaunch;

    [ComImport, Guid("37c994e7-432b-4834-a2f7-dce1f13b834b")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface ITipInvocation
    {
        void Toggle(IntPtr hwnd);
    }
}