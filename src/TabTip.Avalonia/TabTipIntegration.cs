using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Input;
using Avalonia.Media;
using TabTip.Avalonia.TabTip;

namespace TabTip.Avalonia;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
/// <summary>
/// TabTip Integration that opens the keyboard whenever a TextBox gets focused
/// and closes it when a TextBox no longer has focus.
/// </summary>
/// <param name="tabTip">The OS-specific <see cref="ITabTip"/> implementation that will be used to open the TabTip.</param>
/// <remarks>
/// Can be inherited and <seealso cref="Integrate"/> can be overriden to customize the integration logic.
/// You could, for example, add logic to trigger for other custom controls that don't inherit <see cref="TextBox"/> to the integration.
/// For implementation details see: 
/// <see href="https://github.com/giard-alexandre/TabTip.Avalonia/blob/main/src/TabTip.Avalonia/TabTipIntegration.cs">this class in GitHub</see>
/// </remarks>
public class TabTipIntegration(ITabTip tabTip) : ITabTipIntegration
{
    private readonly Dictionary<IInputPane, TopLevel> tlMap = new();
    private readonly Subject<(TextBox TextBox, bool DesiredState)> keyboard = new();

    // ReSharper disable once MemberCanBePrivate.Global
    protected bool IsIntegrated { get; set; }

    public ITabTip TabTip { get; set; } = tabTip;
    public PointerType[] Triggers { get; set; } = [PointerType.Touch, PointerType.Pen];

    public virtual void Integrate()
    {
        if (IsIntegrated)
            return;
        IsIntegrated = true;

        Control.LoadedEvent.AddClassHandler<TopLevel>((s, e) =>
        {
            var input = s.InputPane;
            if (input == null)
                return;

            tlMap[input] = s;
            input.StateChanged += InputPaneStateChanged;
        }, handledEventsToo: true);

        Control.UnloadedEvent.AddClassHandler<TopLevel>((s, e) =>
        {
            var input = s.InputPane;
            if (input == null)
                return;

            input.StateChanged -= InputPaneStateChanged;
            tlMap.Remove(input);
        }, handledEventsToo: true);

        InputElement.PointerPressedEvent.AddClassHandler<TextBox>((t, e) =>
        {
            // Check if we should trigger the tabtip or short-circuit early.
            if (ShouldTrigger(e.Pointer.Type))
            {
                keyboard.OnNext((t, true));
            }
        }, handledEventsToo: true);

        InputElement.LostFocusEvent.AddClassHandler<TextBox>((t, _) => keyboard.OnNext((t, false)),
            handledEventsToo: true);

        keyboard.Throttle(TimeSpan.FromMilliseconds(100)).Subscribe(e =>
        {
            var tl = TopLevel.GetTopLevel(e.TextBox);
            if (tl == null)
                return;

            var hwnd = tl.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
            if (hwnd == IntPtr.Zero)
                return;

            var input = tl.InputPane;
            if (input == null)
                return;

            if (e.DesiredState)
            {
                if (input.State == InputPaneState.Closed)
                {
                    TabTip.Toggle(hwnd);
                }
            }
            else
            {
                if (input.State == InputPaneState.Open)
                {
                    TabTip.Toggle(hwnd);
                }
            }
        });
    }

    // TODO: Make configurable so that we can still trigger the tabtip even when a hardware keyboard is connected.
    private bool ShouldTrigger(PointerType pointerType) =>
        Triggers.Contains(pointerType) && !TabTip.Keyboard.IsHardwareKeyboardConnected();

    // Shift content from behind the osk. Could shift the entire window instead.
    //
    // Docs at https://docs.avaloniaui.net/docs/concepts/services/input-pane#occludedrect
    // say e.EndRect/inputPane.OccludedRect should be empty, but they are not.
    private void InputPaneStateChanged(object? sender, InputPaneStateEventArgs e)
    {
        var inputPane = (IInputPane)sender!;
        var tl = tlMap[inputPane];

        if (tl.FocusManager?.GetFocusedElement() is not TextBox ctrl)
            return;

        if (e.NewState == InputPaneState.Open)
        {
            // Get screen position of the bottom-left point for the InputElement
            var ctrlBottomScrn = tl.PointToScreen(ctrl.Bounds.BottomLeft);
            var ctrlBottom = ctrlBottomScrn.ToPoint(tl.RenderScaling);

            // Get the screen position of the top-left point for the TopLevel
            var tlTopCoords = tl.PointToScreen(tl.Bounds.TopLeft).ToPoint(tl.RenderScaling);

            // https://docs.avaloniaui.net/docs/concepts/services/input-pane#occludedrect
            // "Return value is in client coordinates relative to the current top level."
            // Translate osk relative bounds to "screen" bounds.
            var oskBounds = e.EndRect.Translate(tlTopCoords);

            var contains = oskBounds.Contains(ctrlBottom);
            if (contains)
            {
                var diff = oskBounds.TopLeft - ctrlBottom;
                tl.RenderTransform = new TranslateTransform(0, diff.Y);
            }
        }
        else
        {
            if (tl.RenderTransform is not null)
            {
                tl.RenderTransform = null;
            }
        }
    }
}