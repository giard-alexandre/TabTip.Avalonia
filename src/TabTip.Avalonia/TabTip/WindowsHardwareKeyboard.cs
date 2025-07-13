using System.Runtime.InteropServices;

namespace TabTip.Avalonia.TabTip;

public class WindowsHardwareKeyboard : IHardwareKeyboard
{
    [DllImport("user32.dll")]
    private static extern int GetKeyboardType(int nTypeFlag);

    public bool IsHardwareKeyboardConnected()
    {
        try
        {
            // nTypeFlag = 0 checks the keyboard type.
            // A return value of 0 indicates no keyboard.
            // A value greater than 1 typically indicates an enhanced or programmable keyboard.
            // Any non-zero value suggests a physical keyboard is present.
            int keyboardType = GetKeyboardType(0);
            return keyboardType != 0;
        }
        catch
        {
            // In case of any errors with P/Invoke, assume a keyboard is present
            // for a safe fallback on a desktop platform.
            return true; 
        }
    }
}