using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace TabTip.Avalonia.TabTip;

[SupportedOSPlatform("windows")]
public partial class WindowsHardwareKeyboard : IHardwareKeyboard
{
    /// <summary>
    /// Walks every Raw Input keyboard the OS currently reports, drops the virtual ones, and
    /// returns the bitmask of remaining categories. Backed by Raw Input — devices disappear
    /// from this list when physically detached (e.g. a Surface Type Cover folded back),
    /// making it more reliable than WMI for hot-plug.
    /// </summary>
    public HardwareKeyboardType GetConnected()
    {
        var connected = HardwareKeyboardType.None;
        foreach (var name in EnumerateKeyboardDeviceNames())
        {
            connected |= Classify(name);

            // Once both flags are set we can't learn anything more by looking at remaining devices.
            if (connected == (HardwareKeyboardType.Physical | HardwareKeyboardType.BuiltIn))
                break;
        }

        return connected;
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
        "RDP_KBD",         // Remote Desktop stub, present on every Windows install.
        "ConvertedDevice", // kbdhid.sys wrapper over the legacy i8042 controller — present on every PC chipset.
    ];

    /// <summary>
    /// Maps a PnP device name to a category. Returns <see cref="HardwareKeyboardType.None"/>
    /// for known synthetic devices (RDP_KBD, ConvertedDevice, ROOT#-enumerated). Substring
    /// markers are checked first because they appear inside otherwise-real-looking <c>HID#</c>
    /// paths (notably <c>HID#ConvertedDevice</c>).
    /// </summary>
    private static HardwareKeyboardType Classify(string name)
    {
        if (string.IsNullOrEmpty(name))
            return HardwareKeyboardType.None;

        foreach (var marker in VirtualMarkers)
        {
            if (name.Contains(marker, StringComparison.OrdinalIgnoreCase))
                return HardwareKeyboardType.None;
        }

        // ROOT# is the PnP root enumerator, reserved for synthetic / driver-injected devices.
        if (name.StartsWith(@"\\?\ROOT#", StringComparison.OrdinalIgnoreCase))
            return HardwareKeyboardType.None;

        // ACPI# is the ACPI enumerator — built-in laptop / tablet chassis keyboards.
        if (name.StartsWith(@"\\?\ACPI#", StringComparison.OrdinalIgnoreCase))
            return HardwareKeyboardType.BuiltIn;

        // Treat everything else (HID#, including USB / Bluetooth / Surface Type Cover) as
        // an external physical keyboard. We don't require a VID/PID here because some
        // legitimate HID keyboards (notably bus-attached embedded boards) omit them.
        return HardwareKeyboardType.Physical;
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

    [LibraryImport("user32.dll")]
    private static partial uint GetRawInputDeviceList(
        [Out] RAWINPUTDEVICELIST[]? RawInputDeviceList,
        ref uint NumDevices,
        uint Size);

    // EntryPoint pinned to the W variant: the previous CharSet.Unicode was only acting as
    // an A/W lookup hint, not for string marshalling — we pass IntPtr buffers, no strings
    // cross the boundary.
    [LibraryImport("user32.dll", EntryPoint = "GetRawInputDeviceInfoW")]
    private static partial uint GetRawInputDeviceInfo(
        IntPtr hDevice,
        uint uiCommand,
        IntPtr pData,
        ref uint pcbSize);
}
