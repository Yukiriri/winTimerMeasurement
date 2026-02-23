using Windows.Win32;
using Windows.Win32.Foundation;
using Microsoft.Win32;

var windows_display_ver = "";
var windows_lcu_ver     = "";
{
    using var h_key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
    windows_display_ver = h_key?.GetValue("DisplayVersion")?.ToString() ?? "";
    windows_lcu_ver     = h_key?.GetValue("LCUVer")?.ToString() ?? "";
}

var is_global_timer_request = false;
{
    using var h_key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\kernel");
    is_global_timer_request = ((int?)h_key?.GetValue("GlobalTimerResolutionRequests") ?? 0) != 0;
}

var qp_freq = 0ul;
unsafe
{
    var qp_freq2 = 0ul;
    WinApi.QueryPerformanceFrequency((long*)&qp_freq2);
    qp_freq = qp_freq2;
}

Console.CursorVisible = false;
for (;; Thread.Sleep(1000 / 3))
{
    Console.SetCursorPosition(0, 0);
    unsafe
    {
           Console.WriteLine($"""
                              Windows {windows_display_ver} {windows_lcu_ver}
                              
                              GlobalTimerResolutionRequests : {is_global_timer_request}
                              
                              WaitableTimer :
                                  {"Request"      ,7}    | {"Actual",7}
                                  {1000 / 30.0 ,7:N3} ms | {testWaitableTimer(1000 / 30.0),7:N3} ms
                                  {1000 / 60.0 ,7:N3} ms | {testWaitableTimer(1000 / 60.0),7:N3} ms
                                  {1           ,7:N3} ms | {testWaitableTimer(1)          ,7:N3} ms
                                  {0.5         ,7:N3} ms | {testWaitableTimer(0.5)        ,7:N3} ms
                                  {0.001       ,7:N3} ms | {testWaitableTimer(0.001)      ,7:N3} ms
                              
                              WaitableTimerEx :
                                  {"Request"      ,7}    | {"Actual",7}
                                  {1000 / 30.0 ,7:N3} ms | {testWaitableTimerEx(1000 / 30.0),7:N3} ms
                                  {1000 / 60.0 ,7:N3} ms | {testWaitableTimerEx(1000 / 60.0),7:N3} ms
                                  {1           ,7:N3} ms | {testWaitableTimerEx(1)          ,7:N3} ms
                                  {0.5         ,7:N3} ms | {testWaitableTimerEx(0.5)        ,7:N3} ms
                                  {0.001       ,7:N3} ms | {testWaitableTimerEx(0.001)      ,7:N3} ms
                              """);
    }
}


unsafe double testWaitableTimer(double ms)
{
    var h_timer = WinApi.CreateWaitableTimer(null, false);
    if (h_timer != HANDLE.Null)
    {
        return waitTimer(h_timer, ms) * 1e+3 / qp_freq;
    }

    return -1;
}

unsafe double testWaitableTimerEx(double ms)
{
    const uint CREATE_WAITABLE_TIMER_HIGH_RESOLUTION = 0x2;
    const uint TIMER_ALL_ACCESS                      = 0x1F0003;

    var h_timer = WinApi.CreateWaitableTimerEx(null, null, CREATE_WAITABLE_TIMER_HIGH_RESOLUTION, TIMER_ALL_ACCESS);
    if (h_timer != HANDLE.Null)
    {
        return waitTimer(h_timer, ms) * 1e+3 / qp_freq;
    }

    return -1;
}

unsafe ulong waitTimer(HANDLE h_timer, double ms)
{
    var ns = (long)-(ms * 1e+6 / 100);
    if (WinApi.SetWaitableTimer(h_timer, &ns, 0, null, null, false))
    {
        ulong qpc1 = 0, qpc2 = 0;
        WinApi.QueryPerformanceCounter((long*)&qpc1);
        if (WinApi.WaitForSingleObject(h_timer, uint.MaxValue) == 0)
        {
            WinApi.QueryPerformanceCounter((long*)&qpc2);
            return qpc2 - qpc1;
        }
    }

    return uint.MaxValue;
}
