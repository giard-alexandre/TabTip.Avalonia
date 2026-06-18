namespace TabTip.Avalonia.TabTip;

/// <summary>
/// Information about the OS session the app is running in. Orthogonal to
/// <see cref="IHardwareKeyboard"/>: keyboards reflect the physical hardware attached to the
/// machine, session info reflects how the user is reaching that machine.
/// </summary>
public interface ISessionInfo
{
    /// <summary>
    /// Returns <c>true</c> when the current process is hosted in a remote session (e.g.
    /// Remote Desktop, RemoteApp). The remote user's local input devices are unknowable from
    /// inside the session, so callers typically suppress the on-screen keyboard in this case
    /// and let the remote client surface its own input affordances.
    /// </summary>
    bool IsRemoteSession();
}
