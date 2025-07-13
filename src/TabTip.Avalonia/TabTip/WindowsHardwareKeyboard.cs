using System.Management;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace TabTip.Avalonia.TabTip;

[SupportedOSPlatform("windows")]
public class WindowsHardwareKeyboard : IHardwareKeyboard
{
    [DllImport("user32.dll")]
    private static extern int GetKeyboardType(int nTypeFlag);

    public bool IsHardwareKeyboardConnected()
    {
        int kbType;
        try
        {
            // nTypeFlag = 0 checks the keyboard type.
            // A return value of 0 indicates no keyboard.
            // A value greater than 1 typically indicates an enhanced or programmable keyboard.
            // Any non-zero value suggests a physical keyboard is present.
            int keyboardType = GetKeyboardType(0);
            kbType = keyboardType;
            // return keyboardType != 0;
        }
        catch
        {
            // In case of any errors with P/Invoke, assume a keyboard is present
            // for a safe fallback on a desktop platform.
            return true; 
        }
        try
        {
            // Use a more specific query to get PNPDeviceID directly
            var searcher = new ManagementObjectSearcher("SELECT PNPDeviceID FROM Win32_Keyboard");

            foreach (var keyboard in searcher.Get().Cast<ManagementObject>())
            {
                // Get the PnP Device ID for the current keyboard
                var pnpDeviceId = keyboard["PNPDeviceID"]?.ToString();
                Console.WriteLine($"HARDWARE DEVICE: {pnpDeviceId}");

                // A physical device will have a bus-related ID (e.g., USB, HID).
                // A virtual or software keyboard often has an ID starting with "ROOT".
                // We check if the ID is not null and does not start with "ROOT".
                if (!string.IsNullOrEmpty(pnpDeviceId) && !pnpDeviceId.StartsWith("ROOT"))
                {
                    // Found at least one physical keyboard, no need to check further.
                    // return true;
                }
            }
        }
        catch (ManagementException ex)
        {
            Console.WriteLine("An error occurred while querying for keyboards: " + ex.Message);
            // In case of an error, assume no keyboard to be safe.
            return false;
        }

        // If the loop completes without finding a physical keyboard
        return false;
    }
}