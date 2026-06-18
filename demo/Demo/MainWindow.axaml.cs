using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using TabTip.Avalonia;

namespace Demo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var tl = GetTopLevel(this);
        if (tl == null)
            return;

        var hwnd = tl.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (hwnd == IntPtr.Zero)
            return;

        TabTipManager.Toggle(hwnd);
    }

    private void FocusTextBoxButton_OnClick(object? sender, RoutedEventArgs e)
    {
        GlobalTextBox.Focus();
    }
}
