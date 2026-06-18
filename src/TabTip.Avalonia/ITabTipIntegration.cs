using Avalonia.Controls;
using TabTip.Avalonia.TabTip;

namespace TabTip.Avalonia;

public interface ITabTipIntegration
{
    /// <summary>
    /// Wires up the Avalonia event handlers that drive the on-screen keyboard. The set of
    /// events subscribed to is determined by the concrete <see cref="TabTipTriggerPolicy"/>
    /// subtype, so the policy must be supplied here rather than configured later.
    /// </summary>
    void Integrate(TabTipTriggerPolicy policy);

    /// <summary>
    /// Marks a control whose TextBox descendants should trigger the keyboard. Only meaningful
    /// when the supplied policy has <see cref="TabTipTriggerPolicy.Global"/> set to
    /// <c>false</c>; in global mode every TextBox in the app already triggers.
    /// </summary>
    void Register(Control control);

    ITabTip TabTip { get; set; }
}
