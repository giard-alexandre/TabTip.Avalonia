using Avalonia.Controls;
using Avalonia.Input;
using TabTip.Avalonia.TabTip;

namespace TabTip.Avalonia;

public interface ITabTipIntegration
{
    void Integrate(bool global = true);
    void Register(Control control);

    ITabTip TabTip { get; set; }
    
    /// <summary>
    /// The <see cref="PointerType"/>s that will trigger the logic to check if we open the <see cref="TabTip"/>.
    /// </summary>
    PointerType[] Triggers { get; set; }
}