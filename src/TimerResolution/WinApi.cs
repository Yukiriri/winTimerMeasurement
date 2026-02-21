using System.Runtime.InteropServices;

namespace TimerResolution;

public static class WinApi
{
    [DllImport("ntdll.dll")]
    public static extern unsafe int NtQueryTimerResolution(uint* MaximumTime, uint* MinimumTime, uint* CurrentTime);

    [DllImport("ntdll.dll")]
    public static extern unsafe int NtSetTimerResolution(uint DesiredTime, int SetResolution, uint* ActualTime);
}
