using System.Runtime.InteropServices;

namespace ResolutionAdjustment;

public static class WinNtApi
{
    [DllImport("ntdll.dll")]
    public static extern unsafe int NtQueryTimerResolution(uint* MaximumTime, uint* MinimumTime, uint* CurrentTime);

    [DllImport("ntdll.dll")]
    public static extern unsafe int NtSetTimerResolution(uint DesiredTime, int SetResolution, uint* ActualTime);
}
