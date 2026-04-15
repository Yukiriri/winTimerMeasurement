using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using Windows.Win32;
using Windows.Win32.Foundation;
using Microsoft.Win32;
using TimerBench;

Process.GetCurrentProcess().ProcessorAffinity = (nint)1;

var windows_display_ver = "";
var windows_lcu_ver     = "";
{
    using var h_key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
    windows_display_ver = h_key?.GetValue("DisplayVersion")?.ToString() ?? "";
    windows_lcu_ver     = h_key?.GetValue("LCUVer")?.ToString() ?? "";
}
var qp_freq = 0ul;
unsafe
{
    WinApi.QueryPerformanceFrequency((long*)&qp_freq);
}

Console.CursorVisible = false;
for (;;)
{
    Console.SetCursorPosition(0, 0);
    unsafe
    {
        var qpc1 = getCounterValue<long, BOOL>(&WinApi.QueryPerformanceCounter);
        var qit1 = getCounterValue_void<ulong>(&WinApi.QueryInterruptTime);
        Thread.Sleep(1000);
        var qpc2 = getCounterValue<long, BOOL>(&WinApi.QueryPerformanceCounter);
        var qit2 = getCounterValue_void<ulong>(&WinApi.QueryInterruptTime);

        Console.WriteLine($"""
                           Windows {windows_display_ver} {windows_lcu_ver}
                           
                           QueryPerformanceFrequency      : {qp_freq / 1e+6,7:N3} MHz
                           
                           QueryPerformanceCounter        : {testCounterPrecision<long, BOOL>(&WinApi.QueryPerformanceCounter) * (1e+9 / qp_freq),7} ns
                           GetSystemTimePreciseAsFileTime : {testCounterPrecision_void<FILETIME>(&WinApi.GetSystemTimePreciseAsFileTime) * 100   ,7} ns
                           
                           QueryInterruptTime             : {testCounterPrecision_void<ulong>(&WinApi.QueryInterruptTime)         * 100 / 1e+6,7:N3} ms
                           GetSystemTimeAsFileTime        : {testCounterPrecision_void<FILETIME>(&WinApi.GetSystemTimeAsFileTime) * 100 / 1e+6,7:N3} ms
                           
                           QueryPerformanceCounter        : Δ {qpc2 - qpc1}
                           QueryInterruptTime             : Δ {qit2 - qit1}

                           """);
    }
}


unsafe ulong testCounterPrecision<T, TResult>(delegate* managed<T*, TResult> query_func)
{
    ulong q1 = 0, q2 = 0;
    query_func((T*)&q1);
    while (q1 >= q2)
        query_func((T*)&q2);
    return q2 - q1;
}

unsafe ulong testCounterPrecision_void<T>(delegate* managed<T*, void> query_func)
{
    ulong q1 = 0, q2 = 0;
    query_func((T*)&q1);
    while (q1 >= q2)
        query_func((T*)&q2);
    return q2 - q1;
}

unsafe ulong getCounterValue<T, TResult>(delegate* managed<T*, TResult> query_func)
{
    ulong ret = 0;
    query_func((T*)&ret);
    return ret;
}

unsafe ulong getCounterValue_void<T>(delegate* managed<T*, void> query_func)
{
    ulong ret = 0;
    query_func((T*)&ret);
    return ret;
}
