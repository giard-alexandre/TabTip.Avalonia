using Avalonia;
using Avalonia.Controls;
using TabTip.Avalonia.TabTip;

namespace TabTip.Avalonia;

public static class TabTipManager
{
    private static ITabTipFactory _tabTipFactory = new DefaultTabTipFactory();
    private static ITabTip _tabTip = _tabTipFactory.Create();
    private static ITabTipIntegration _integration = new TabTipIntegration(_tabTip);

    /// <summary>
    /// Attached property to register a control for TabTip integration via XAML.
    /// </summary>
    public static readonly AttachedProperty<bool> IsRegisteredProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>("IsRegistered", typeof(TabTipManager), false);

    /// <summary>
    /// Gets the value of the IsRegistered attached property.
    /// </summary>
    public static bool GetIsRegistered(Control control) => control.GetValue(IsRegisteredProperty);

    /// <summary>
    /// Sets the value of the IsRegistered attached property.
    /// </summary>
    public static void SetIsRegistered(Control control, bool value) => control.SetValue(IsRegisteredProperty, value);

    static TabTipManager()
    {
        IsRegisteredProperty.Changed.AddClassHandler<Control>(OnIsRegisteredChanged);
    }

    private static void OnIsRegisteredChanged(Control control, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is true)
        {
            Register(control);
        }
    }

    public static void Register(Control control)
    {
        _integration.Register(control);
    }

    /// <summary>
    /// Wires the integration up using the supplied trigger policy. Pass a
    /// <see cref="PointerOnlyTriggerPolicy"/> for the default pointer-driven behavior (Touch
    /// + Pen by default; programmatic focus is not handled), or a
    /// <see cref="KeyboardDetectionTriggerPolicy"/> to open on any TextBox focus when no
    /// hardware keyboard is detected. Set <see cref="TabTipTriggerPolicy.Global"/> to
    /// <c>false</c> to limit triggering to controls passed to <see cref="Register"/>.
    /// </summary>
    public static void Integrate(TabTipTriggerPolicy policy)
    {
        _integration.Integrate(policy);
    }

    public static void Toggle(IntPtr hwnd)
    {
        _integration.TabTip.Toggle(hwnd);
    }

    public static void OverrideTabTipFactory(ITabTipFactory factory)
    {
        _tabTipFactory = factory;
        OverrideTabTip(factory.Create());
    }

    public static void OverrideTabTip(ITabTip tabTip)
    {
        _tabTip = tabTip;
        _integration.TabTip = _tabTip;
    }

    public static void OverrideIntegration(ITabTipIntegration integration)
    {
        _integration = integration;
    }
}
