using System.Runtime.InteropServices.ComTypes;
using Windows.Win32;
using Windows.Win32.Foundation;
using Microsoft.Win32;

var windows_display_ver = "";
var windows_lcu_ver     = "";
{
    using var h_key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
    windows_display_ver = h_key?.GetValue("DisplayVersion")?.ToString() ?? "";
    windows_lcu_ver     = h_key?.GetValue("LCUVer")?.ToString()         ?? "";
}

var qp_freq = 0ul;
unsafe
{
    WinApi.QueryPerformanceFrequency((long*)&qp_freq);
}

Console.CursorVisible = false;
for (;; Thread.Sleep(1000 / 3))
{
    Console.SetCursorPosition(0, 0);
    unsafe
    {
          Console.WriteLine($"""
                             Windows {windows_display_ver} {windows_lcu_ver}
                             
                             QueryPerformanceFrequency           : {qp_freq} hz
                             
                             QueryPerformanceCounter             : {testPrecision<long, BOOL>(&WinApi.QueryPerformanceCounter) * 1e+9 / qp_freq,7} ns
                             QueryInterruptTimePrecise           : {testPrecision_void<ulong>(&WinApi.QueryInterruptTimePrecise) * 100         ,7} ns
                             QueryUnbiasedInterruptTimePrecise   : {testPrecision_void<ulong>(&WinApi.QueryUnbiasedInterruptTimePrecise) * 100 ,7} ns
                             GetSystemTimePreciseAsFileTime      : {testPrecision_void<FILETIME>(&WinApi.GetSystemTimePreciseAsFileTime) * 100 ,7} ns
                             
                             QueryInterruptTime                  : {testPrecision_void<ulong>(&WinApi.QueryInterruptTime) * 100 / 1e+6         ,7:N3} ms
                             QueryUnbiasedInterruptTime          : {testPrecision<ulong, BOOL>(&WinApi.QueryUnbiasedInterruptTime) * 100 / 1e+6,7:N3} ms
                             GetSystemTimeAsFileTime             : {testPrecision_void<FILETIME>(&WinApi.GetSystemTimeAsFileTime) * 100 / 1e+6 ,7:N3} ms
                             """);
    }
}


unsafe ulong testPrecision<T, TResult>(delegate* managed<T*, TResult> query_func)
{
    ulong q1 = 0, q2 = 0;
    query_func((T*)&q1);
    while (q1 >= q2)
        query_func((T*)&q2);
    return q2 - q1;
}

unsafe ulong testPrecision_void<T>(delegate* managed<T*, void> query_func)
{
    ulong q1 = 0, q2 = 0;
    query_func((T*)&q1);
    while (q1 >= q2)
        query_func((T*)&q2);
    return q2 - q1;
}
