using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.Versioning;

namespace TabTip.Avalonia.TabTip;

[SupportedOSPlatform("windows")]
public partial class WindowsTabTip : ITabTip
{
    private static readonly Guid UIHostNoLaunch_ClsId = new Guid("4ce576fa-83dc-4F88-951c-9d0782b4e376");
    private const string ITipInvocation_IIdRaw = "37c994e7-432b-4834-a2f7-dce1f13b834b";
    private static readonly Guid ITipInvocation_IId = new Guid(ITipInvocation_IIdRaw);

    private const int ClassNotRegistered = unchecked((int)0x80040154);
    private const uint CLSCTX_LOCAL_SERVER = 0x4;

    public IHardwareKeyboard Keyboard { get; } = new WindowsHardwareKeyboard();
    public ISessionInfo Session { get; } = new WindowsSessionInfo();

    public void Toggle(IntPtr hwnd)
    {
        int hr = Ole32.CoCreateInstance(UIHostNoLaunch_ClsId, 0, CLSCTX_LOCAL_SERVER, ITipInvocation_IId, out nint ptr);
        if (hr == ClassNotRegistered)
        {
            // The process was not started before, so we start it.
            Process p = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "tabtip.exe",
                    UseShellExecute = true
                }
            };
            p.Start();
            return;
        }
        else Marshal.ThrowExceptionForHR(hr);

        var comWrappers = new StrategyBasedComWrappers();

        // ReSharper disable once SuspiciousTypeConversion.Global
        var tipInvocation = (ITipInvocation)comWrappers.GetOrCreateObjectForComInstance(ptr, CreateObjectFlags.None);
        tipInvocation.Toggle(hwnd);
        Marshal.Release(ptr);
    }

    [GeneratedComInterface, Guid(ITipInvocation_IIdRaw)]
    internal partial interface ITipInvocation
    {
        void Toggle(IntPtr hwnd);
    }

    internal static partial class Ole32
    {
        [LibraryImport("ole32.dll")]
        public static partial int CoCreateInstance(Guid rclsid, nint pUnkOuter, uint dwClsContext, Guid riid, out nint ppv);
    }
}