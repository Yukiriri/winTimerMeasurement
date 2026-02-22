using Windows.Win32;
using Windows.Win32.Foundation;
using Microsoft.Win32;

var windows_display_ver = "";
var windows_lcu_ver = "";
{
    using var h_key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
    if (h_key != null)
    {
        windows_display_ver += h_key.GetValue("DisplayVersion", "").ToString();
        windows_lcu_ver += h_key.GetValue("LCUVer", "").ToString();
    }
}

var is_global_timer_request = false;
{
    using var h_key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\kernel");
    if (h_key != null)
        is_global_timer_request = (int)h_key.GetValue("GlobalTimerResolutionRequests", 0) != 0;
}

var qp_freq = 0ul;
unsafe
{
    var qp_freq2 = 0ul;
    WinApi.QueryPerformanceFrequency((long*)&qp_freq2);
    qp_freq = qp_freq2;
}

Console.CursorVisible = false;
for (;; Thread.Sleep(1))
{
    Console.SetCursorPosition(0, 0);

    Console.WriteLine($"Windows {windows_display_ver} {windows_lcu_ver} \n");
    
    Console.WriteLine($"GlobalTimerResolutionRequests : {is_global_timer_request} \n");
    
    unsafe
    {
        const uint CREATE_WAITABLE_TIMER_HIGH_RESOLUTION = 0x2;
        const uint TIMER_ALL_ACCESS = 0x1F0003;
        var h_timers = new[]
        {
            ("WaitableTimer", WinApi.CreateWaitableTimer(bManualReset: false)),
            ("WaitableTimerEx", WinApi.CreateWaitableTimerEx(
                dwFlags: CREATE_WAITABLE_TIMER_HIGH_RESOLUTION, 
                dwDesiredAccess: TIMER_ALL_ACCESS))
        };
        
        foreach (var h_timer in h_timers)
        {
            Console.WriteLine($"{h_timer.Item1,-15} : {"Request",7+3} | {"Actual",7+3}");
            if (h_timer.Item2 != HANDLE.Null)
            {
                testWaitableTimer(h_timer.Item2, 1000 / 30.0);
                testWaitableTimer(h_timer.Item2, 1000 / 60.0);
                testWaitableTimer(h_timer.Item2, 1);
                testWaitableTimer(h_timer.Item2, 0.5);
                testWaitableTimer(h_timer.Item2, 0.001);
                WinApi.CloseHandle(h_timer.Item2);
            }
            Console.WriteLine();
        }
    }
}

unsafe void testWaitableTimer(HANDLE h_timer, double ms)
{
    for (var i = DateTimeOffset.Now.ToUnixTimeMilliseconds(); 
         DateTimeOffset.Now.ToUnixTimeMilliseconds() - i < 1000;)
    {
        var ns = (long)-(ms * 1e+6 / 100);
        if (WinApi.SetWaitableTimer(h_timer, &ns, 0, fResume: false))
        {
            ulong qpc1 = 0, qpc2 = 0;
            WinApi.QueryPerformanceCounter((long*)&qpc1);
            if (WinApi.WaitForSingleObject(h_timer, uint.MaxValue) == 0)
            {
                WinApi.QueryPerformanceCounter((long*)&qpc2);
                Console.Write($"{"",-15}   {ms,7:N3} ms | {(qpc2 - qpc1) * 1e+3 / qp_freq,7:N3} ms \r");
            }
        }
    }
    Console.WriteLine();
}
