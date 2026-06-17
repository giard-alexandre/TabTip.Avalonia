using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace TabTip.Avalonia.TabTip;

[SupportedOSPlatform("windows")]
public class WindowsHardwareKeyboard : IHardwareKeyboard
{
    /// <summary>
    /// Returns true when at least one keyboard the OS reports is a real, physical device.
    /// Backed by Raw Input — devices disappear from this list when physically detached
    /// (e.g. a Surface Type Cover folded back), making it more reliable than WMI for hot-plug.
    /// </summary>
    public bool IsHardwareKeyboardConnected()
    {
        foreach (var name in EnumerateKeyboardDeviceNames())
        {
            if (!IsVirtualDevice(name))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Yields the PnP device name of every Raw Input keyboard currently registered with the OS.
    /// </summary>
    private static IEnumerable<string> EnumerateKeyboardDeviceNames()
    {
        uint count = 0;
        var sizeofList = (uint)Marshal.SizeOf<RAWINPUTDEVICELIST>();

        if (GetRawInputDeviceList(null, ref count, sizeofList) != 0 || count == 0)
            yield break;

        var devices = new RAWINPUTDEVICELIST[count];
        if (GetRawInputDeviceList(devices, ref count, sizeofList) == ApiError)
            yield break;

        foreach (var device in devices)
        {
            if (device.dwType != RIM_TYPEKEYBOARD)
                continue;

            yield return GetDeviceName(device.hDevice);
        }
    }

    // Substrings of PnP device names that identify a non-physical keyboard. Add new
    // false-positive enumerator IDs here when they're discovered.
    private static readonly string[] VirtualMarkers =
    [
        "RDP_KBD", // Remote Desktop stub, present on every Windows install.
        "ConvertedDevice", // kbdhid.sys wrapper over the legacy i8042 controller — present on every PC chipset.
    ];

    /// <summary>
    /// Returns true if a PnP device name corresponds to a known software/synthetic keyboard
    /// rather than a real attached one. Substrings (RDP_KBD / ConvertedDevice) are checked
    /// first because they appear inside otherwise-real-looking <c>HID#</c> paths; <c>ROOT#</c>
    /// is the PnP root enumerator, reserved for synthetic devices.
    /// </summary>
    private static bool IsVirtualDevice(string name)
    {
        if (string.IsNullOrEmpty(name))
            return true;

        foreach (var marker in VirtualMarkers)
        {
            if (name.Contains(marker, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return name.StartsWith(@"\\?\ROOT#", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Reads the PnP device name (e.g. <c>\\?\HID#VID_046D&amp;PID_C534&amp;...</c>) for a Raw Input
    /// device handle. Two-call pattern: first call queries the required buffer size, second
    /// call fills it. Returns <see cref="string.Empty"/> on any failure rather than throwing —
    /// device enumeration is best-effort, not a correctness boundary.
    /// </summary>
    private static string GetDeviceName(IntPtr hDevice)
    {
        uint nameLen = 0;
        if (GetRawInputDeviceInfo(hDevice, RIDI_DEVICENAME, IntPtr.Zero, ref nameLen) != 0
            || nameLen == 0)
            return string.Empty;

        var buffer = Marshal.AllocHGlobal((int)(nameLen * sizeof(char)));
        try
        {
            if (GetRawInputDeviceInfo(hDevice, RIDI_DEVICENAME, buffer, ref nameLen) == ApiError)
                return string.Empty;

            return Marshal.PtrToStringUni(buffer) ?? string.Empty;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    // Win32 sentinel returned by GetRawInputDevice* on failure.
    private const uint ApiError = unchecked((uint)-1);

    private const uint RIM_TYPEKEYBOARD = 1;
    private const uint RIDI_DEVICENAME = 0x20000007;

    [StructLayout(LayoutKind.Sequential)]
    private struct RAWINPUTDEVICELIST
    {
        public IntPtr hDevice;
        public uint dwType;
    }

    [DllImport("user32.dll")]
    private static extern uint GetRawInputDeviceList(
        [In, Out] RAWINPUTDEVICELIST[]? RawInputDeviceList,
        ref uint NumDevices,
        uint Size);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern uint GetRawInputDeviceInfo(
        IntPtr hDevice,
        uint uiCommand,
        IntPtr pData,
        ref uint pcbSize);
}
