namespace TabTip.Avalonia.TabTip;

/// <summary>
/// Categories of physical keyboard the OS may report as connected. Used as a flags set so
/// callers can ask "is at least one of these types attached?" with a single bitmask test.
/// Virtual / synthetic devices (RDP, i8042 stubs, ROOT-enumerated) are filtered out before
/// classification and never appear in the result.
/// </summary>
[Flags]
public enum HardwareKeyboardType
{
    None = 0,

    /// <summary>External keyboard: USB, Bluetooth, or wireless dongle (HID enumerator with VID/PID).</summary>
    Physical = 1 << 0,

    /// <summary>Built-in laptop / tablet chassis keyboard (ACPI enumerator).</summary>
    BuiltIn = 1 << 1,
}
