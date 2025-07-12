namespace TabTip.Avalonia;

public interface ITabTipIntegration
{
    void Integrate();
    ITabTip TabTip { get; set; }
}