using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Input;
using Avalonia.Media;

namespace TabTip.Avalonia;

public class TabTipIntegration
{
    private static readonly ITabTip _tabTip;
    private static readonly Dictionary<IInputPane, TopLevel> tlMap = new();
    private static readonly Subject<(InputElement InputElement, bool DesiredState)> keyboard = new();
    private static bool _alreadyDone;

    public static void Integrate()
    {
        if (_alreadyDone)
            return;
        _alreadyDone = true;

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
            // if (e.Pointer.Type == PointerType.Touch)
                keyboard.OnNext((t, true));
            
            // TODO: Restore below (probably)
            // if (e.Pointer.Type == PointerType.Touch)
            //     keyboard.OnNext((t, true));
        }, handledEventsToo: true);

        InputElement.LostFocusEvent.AddClassHandler<TextBox>((t, _) => keyboard.OnNext((t, false)), handledEventsToo: true);

        keyboard.Throttle(TimeSpan.FromMilliseconds(100)).Subscribe(e =>
        {
            var tl = TopLevel.GetTopLevel(e.InputElement);
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
                    _tabTip.Toggle(hwnd);
                }
            }
            else
            {
                if (input.State == InputPaneState.Open)
                {
                    _tabTip.Toggle(hwnd);
                }
            }
        });
    }

    // Shift content from behind the osk. Could shift the entire window instead.
    //
    // Docs at https://docs.avaloniaui.net/docs/concepts/services/input-pane#occludedrect
    // say e.EndRect/inputPane.OccludedRect should be empty, but they are not.
    private static void InputPaneStateChanged(object? sender, InputPaneStateEventArgs e)
    {
        var inputPane = (IInputPane)sender!;
        var tl = tlMap[inputPane];

        if (tl.FocusManager?.GetFocusedElement() is not InputElement ctrl) // TODO: Use textbox?
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