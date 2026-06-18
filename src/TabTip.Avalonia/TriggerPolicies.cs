using Avalonia.Input;
using TabTip.Avalonia.TabTip;

namespace TabTip.Avalonia;

/// <summary>
/// Configures how the on-screen keyboard is triggered. Pass an instance to
/// <see cref="TabTipManager.Integrate"/> — the integration dispatches on the concrete subtype
/// to decide which Avalonia events to subscribe to. The hierarchy is closed: only the two
/// built-in subclasses are valid.
/// </summary>
public abstract class TabTipTriggerPolicy
{
    private protected TabTipTriggerPolicy() { }

    /// <summary>
    /// When <c>true</c> (default), the policy applies to every TextBox in the application.
    /// When <c>false</c>, only TextBoxes that live inside a control registered via
    /// <see cref="TabTipManager.Register"/> trigger the keyboard.
    /// </summary>
    public bool Global { get; set; } = true;
}

/// <summary>
/// Open the keyboard only when a TextBox is pressed by one of the configured pointer types
/// (Touch + Pen by default). Hardware-keyboard detection is not consulted, so behavior is
/// identical on a desktop with a keyboard and on a tablet without one.
/// </summary>
/// <remarks>
/// <b>Pointer mode does not support the programmatic-focus workflow.</b> A code-driven
/// <c>control.Focus()</c> produces no pointer event, and there is no pointer-type signal to
/// gate on, so this policy ignores those focus changes by design. If you need the keyboard to
/// open on programmatic focus, use <see cref="KeyboardDetectionTriggerPolicy"/>.
/// </remarks>
public sealed class PointerOnlyTriggerPolicy : TabTipTriggerPolicy
{
    public HashSet<PointerType> Triggers { get; set; } = [PointerType.Touch, PointerType.Pen];
}

/// <summary>
/// "Just integrate and it does the right thing" — open the keyboard on any TextBox focus
/// (pointer or programmatic) unless a keyboard the caller cares about is attached. Best
/// suited to convertible / 2-in-1 devices that switch modes at runtime.
/// </summary>
/// <remarks>
/// Pointer presses are not subscribed to in this mode; focus is the single source of truth
/// and the hardware-keyboard probe is the only gate.
/// </remarks>
public sealed class KeyboardDetectionTriggerPolicy : TabTipTriggerPolicy
{
    /// <summary>
    /// Keyboard categories whose presence suppresses the on-screen keyboard. Defaults to
    /// <see cref="HardwareKeyboardType.Physical"/> only — built-in chassis keyboards are
    /// ignored, so a 2-in-1 with its keyboard folded back still pops the OSK even though
    /// the laptop's built-in keyboard is technically still enumerated. Set to
    /// <c>Physical | BuiltIn</c> to also suppress when a built-in keyboard is detected, or
    /// to <see cref="HardwareKeyboardType.None"/> to always show the OSK on focus.
    /// </summary>
    public HardwareKeyboardType SuppressOn { get; set; } = HardwareKeyboardType.Physical;

    /// <summary>
    /// When <c>true</c> (default), suppress the on-screen keyboard whenever the process is
    /// running inside a remote session (Remote Desktop, RemoteApp, etc.). The remote user's
    /// real input devices are unknowable from inside the session, and the remote client
    /// already surfaces its own input affordances, so popping the host's OSK is usually
    /// noise. Set to <c>false</c> to ignore session state and decide purely on the
    /// <see cref="SuppressOn"/> keyboard set.
    /// </summary>
    public bool SuppressOnRemoteSession { get; set; } = true;
}
