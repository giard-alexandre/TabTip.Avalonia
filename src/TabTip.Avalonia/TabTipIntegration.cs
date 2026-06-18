using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
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
    private readonly HashSet<Control> registeredControls = [];
    private readonly Subject<(TextBox TextBox, bool DesiredState)> keyboard = new();
    private bool _occlusionManagerRegistered;

    // ReSharper disable once MemberCanBePrivate.Global
    protected bool IsIntegrated { get; set; }

    public ITabTip TabTip { get; set; } = tabTip;

    /// <inheritdoc />
    public void Register(Control control)
    {
        registeredControls.Add(control);
    }

    private bool IsChildOfRegisteredControls(TextBox eventingTextBox) =>
        registeredControls.Any(c => c == eventingTextBox || c.IsVisualAncestorOf(eventingTextBox));

    public virtual void Integrate(TabTipTriggerPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        RegisterOcclusionManager();

        if (IsIntegrated)
            return;
        IsIntegrated = true;

        var global = policy.Global;

        switch (policy)
        {
            case PointerOnlyTriggerPolicy pointerPolicy:
                // Pointer-only: gate on the configured pointer types. Programmatic focus is
                // not handled in this mode — see PointerOnlyTriggerPolicy remarks.
                InputElement.PointerPressedEvent.AddClassHandler<TextBox>((t, e) =>
                {
                    if (pointerPolicy.Triggers.Contains(e.Pointer.Type)
                        && (global || IsChildOfRegisteredControls(t)))
                    {
                        keyboard.OnNext((t, true));
                    }
                }, handledEventsToo: true);
                break;

            case KeyboardDetectionTriggerPolicy detectionPolicy:
                // Keyboard-detection: focus is the only signal we need. Open whenever a
                // TextBox gets focus (pointer or programmatic) and neither a configured
                // keyboard category nor a remote session is suppressing. Pointer presses
                // aren't subscribed to in this mode.
                InputElement.GotFocusEvent.AddClassHandler<TextBox>((t, _) =>
                {
                    if (detectionPolicy.SuppressOnRemoteSession && TabTip.Session.IsRemoteSession())
                        return;

                    var connected = TabTip.Keyboard.GetConnected();
                    if ((connected & detectionPolicy.SuppressOn) == HardwareKeyboardType.None
                        && (global || IsChildOfRegisteredControls(t)))
                    {
                        keyboard.OnNext((t, true));
                    }
                }, handledEventsToo: true);
                break;

            default:
                // Defensive: TabTipTriggerPolicy has a private protected ctor so external
                // subclasses can't reach it, but a same-assembly addition would slip past
                // the type system here. Fail loudly rather than silently no-op.
                throw new NotSupportedException(
                    $"Unsupported trigger policy: {policy.GetType().FullName}. " +
                    $"Use {nameof(PointerOnlyTriggerPolicy)} or {nameof(KeyboardDetectionTriggerPolicy)}.");
        }

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

    private void RegisterOcclusionManager()
    {
        if (_occlusionManagerRegistered)
            return;

        _occlusionManagerRegistered = true;

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
    }

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
