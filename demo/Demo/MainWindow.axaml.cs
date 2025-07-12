using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Demo;

public partial class MainWindow : Window
{
    // static MainWindow()
    // {
    //     OSKIntegration.Integrate();
    // }
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

        // OSKIntegration.ToggleOSK(hwnd);
    }
}