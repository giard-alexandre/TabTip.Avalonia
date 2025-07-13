namespace TabTip.Avalonia.TabTip;

/// <summary>
/// Defines a service to detect the presence of a hardware keyboard.
/// </summary>
public interface IHardwareKeyboard
{
    /// <summary>
    /// Gets a value indicating whether a physical hardware keyboard is connected.
    /// </summary>
    bool IsHardwareKeyboardConnected();
}