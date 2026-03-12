using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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

    public static void Integrate(bool global = true)
    {
        _integration.Integrate(global);
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

    public static void OverrideIntegrationTrigger(PointerType[] triggers)
    {
        _integration.Triggers = triggers;
    }
}
