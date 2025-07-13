using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using TabTip.Avalonia;

namespace Demo;

public partial class MainWindow : Window
{
    static MainWindow()
    {
        // Integrate into every InputControl in the app.
        TabTipManager.Integrate();
    }
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        TopLevel? tl = GetTopLevel(this);
        if (tl == null)
            return;

        IntPtr hwnd = tl.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (hwnd == IntPtr.Zero)
            return;

        TabTipManager.Toggle(hwnd);
    }
}