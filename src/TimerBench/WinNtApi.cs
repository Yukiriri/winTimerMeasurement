using System.Runtime.InteropServices;

namespace TimerBench;

public static class WinNtApi
{
    [DllImport("ntdll.dll")]
    public static extern unsafe int NtQueryPerformanceCounter(ulong* PerformanceCounter, ulong* PerformanceFrequency);

    // [DllImport("hal.dll")]
    // public static extern unsafe ulong KeQueryPerformanceCounter(ulong* PerformanceFrequency);
}
