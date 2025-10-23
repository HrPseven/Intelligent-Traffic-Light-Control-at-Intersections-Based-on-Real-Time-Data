using UnityEngine;
using System.Runtime.InteropServices;

public class PreventSystemSleep : MonoBehaviour
{
    // Import the native Windows API function
    [DllImport("kernel32.dll")]
    static extern uint SetThreadExecutionState(uint esFlags);

    // Flags to prevent sleep and display off
    const uint ES_CONTINUOUS        = 0x80000000;
    const uint ES_SYSTEM_REQUIRED   = 0x00000001;
    const uint ES_DISPLAY_REQUIRED  = 0x00000002;

    void Start()
    {
        // Prevent system sleep and screen dimming
        SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED);

        // Prevent Unity from pausing in background
        // Application.runInBackground = true;

        // Optional for mobile or Unity screen settings
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        Debug.Log("🛑 System sleep and screen dimming prevented.");
    }

    void OnApplicationQuit()
    {
        // Re-enable sleep when the app closes
        SetThreadExecutionState(ES_CONTINUOUS);
    }
}