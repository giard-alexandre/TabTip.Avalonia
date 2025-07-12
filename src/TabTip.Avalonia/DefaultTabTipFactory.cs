namespace TabTip.Avalonia;

public class DefaultTabTipFactory : ITabTipFactory
{
    public ITabTip Create()
    {
        if (OperatingSystem.IsWindows())
        {
            return new WindowsTabTip();
        }

        // Ignore other OSs for now.
        return new NullTabTip();
    }
}