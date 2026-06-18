namespace TabTip.Avalonia.TabTip;

/// <summary>
/// Defines a service to detect the presence of hardware keyboards.
/// </summary>
public interface IHardwareKeyboard
{
    /// <summary>
    /// Returns the bitmask of <see cref="HardwareKeyboardType"/>s currently attached to
    /// the device. Returns <see cref="HardwareKeyboardType.None"/> when nothing physical
    /// is detected.
    /// </summary>
    HardwareKeyboardType GetConnected();
}
