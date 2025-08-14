using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using TabTip.Avalonia;

namespace Targeted.Demo;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        // Integrate the tabtip manager non-globally and override the triggers to include all pointer types..
        TabTipManager.OverrideIntegrationTrigger([PointerType.Touch, PointerType.Mouse, PointerType.Pen]);
        TabTipManager.Integrate(false);

        base.OnFrameworkInitializationCompleted();
    }
}