using Avalonia.Controls;
using Avalonia.Input;
using TabTip.Avalonia.TabTip;

namespace TabTip.Avalonia;

public static class TabTipManager
{
    private static ITabTipFactory _tabTipFactory = new DefaultTabTipFactory();
    private static ITabTip _tabTip = _tabTipFactory.Create();
    private static ITabTipIntegration _integration = new TabTipIntegration(_tabTip);

    public static void Register(Control control)
    {
        _integration.Register(control);
    }

    public static void Integrate()
    {
        _integration.Integrate();
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