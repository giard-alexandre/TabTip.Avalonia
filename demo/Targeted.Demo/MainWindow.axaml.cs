using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using TabTip.Avalonia;

namespace Targeted.Demo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        TabTipManager.Register(IntegratedTextBox);
        TabTipManager.Register(IntegratedNumericUpDown);
        TabTipManager.Register(IntegratedAutoCompleteBox);
        TabTipManager.Register(IntegrateHeaderedContentControl);
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