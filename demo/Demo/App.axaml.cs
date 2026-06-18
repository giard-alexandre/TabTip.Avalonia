using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using TabTip.Avalonia;
using TabTip.Avalonia.TabTip;

namespace Demo;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Integrate non-globally and disable keyboard type suppression, so the
        // demo is exercisable on a desktop without a touch screen.
        TabTipManager.Integrate(new KeyboardDetectionTriggerPolicy
        {
            Global = true,
            SuppressOnRemoteSession = false,
            SuppressOn = HardwareKeyboardType.None,
        });

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
