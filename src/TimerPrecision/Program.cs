using System.Runtime.InteropServices.ComTypes;
using Windows.Win32;
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

var qp_freq = 0ul;
unsafe
{
    WinApi.QueryPerformanceFrequency((long*)&qp_freq);
}

Console.CursorVisible = false;
for (;; Thread.Sleep(1000 / 10))
{
    Console.SetCursorPosition(0, 0);

    Console.WriteLine($"Windows {windows_display_ver} {windows_lcu_ver} \n");

    Console.WriteLine($"{qp_freq,9} hz : QueryPerformanceFrequency \n");

    unsafe
    {
        ulong qpc1 = 0, qpc2 = 0;
        WinApi.QueryPerformanceCounter((long*)&qpc1);
        while (qpc1 >= qpc2)
            WinApi.QueryPerformanceCounter((long*)&qpc2);
        Console.WriteLine($"{(qpc2 - qpc1) * 1e+9 / qp_freq,9} ns : QueryPerformanceCounter");

        ulong qitp1 = 0, qitp2 = 0;
        WinApi.QueryInterruptTimePrecise(&qitp1);
        while (qitp1 >= qitp2)
            WinApi.QueryInterruptTimePrecise(&qitp2);
        Console.WriteLine($"{(qitp2 - qitp1) * 100,9} ns : QueryInterruptTimePrecise");

        ulong quitp1 = 0, quitp2 = 0;
        WinApi.QueryUnbiasedInterruptTimePrecise(&quitp1);
        while (quitp1 >= quitp2)
            WinApi.QueryUnbiasedInterruptTimePrecise(&quitp2);
        Console.WriteLine($"{(quitp2 - quitp1) * 100,9} ns : QueryUnbiasedInterruptTimePrecise");

        ulong stp1 = 0, stp2 = 0;
        WinApi.GetSystemTimePreciseAsFileTime((FILETIME*)&stp1);
        while (stp1 >= stp2)
            WinApi.GetSystemTimePreciseAsFileTime((FILETIME*)&stp2);
        Console.WriteLine($"{(stp2 - stp1) * 100,9} ns : GetSystemTimePreciseAsFileTime");

        Console.WriteLine();

        ulong qit1 = 0, qit2 = 0;
        WinApi.QueryInterruptTime(&qit1);
        while (qit1 >= qit2)
            WinApi.QueryInterruptTime(&qit2);
        Console.WriteLine($"{(qit2 - qit1) * 100 / 1e+6,9:N3} ms : QueryInterruptTime");

        ulong quit1 = 0, quit2 = 0;
        WinApi.QueryUnbiasedInterruptTime(&quit1);
        while (quit1 >= quit2)
            WinApi.QueryUnbiasedInterruptTime(&quit2);
        Console.WriteLine($"{(quit2 - quit1) * 100 / 1e+6,9:N3} ms : QueryUnbiasedInterruptTime");

        ulong gst1 = 0, gst2 = 0;
        WinApi.GetSystemTimeAsFileTime((FILETIME*)&gst1);
        while (gst1 >= gst2)
            WinApi.GetSystemTimeAsFileTime((FILETIME*)&gst2);
        Console.WriteLine($"{(gst2 - gst1) * 100 / 1e+6,9:N3} ms : GetSystemTimeAsFileTime");

        Console.WriteLine();

        ulong gtc64_1 = 0, gtc64_2 = 0;
        gtc64_1 = WinApi.GetTickCount64();
        while (gtc64_1 >= gtc64_2)
            gtc64_2 = WinApi.GetTickCount64();
        Console.WriteLine($"{gtc64_2 - gtc64_1,9} ms : GetTickCount64");

        uint gtc_1 = 0, gtc_2 = 0;
        gtc_1 = WinApi.GetTickCount();
        while (gtc_1 >= gtc_2)
            gtc_2 = WinApi.GetTickCount();
        Console.WriteLine($"{gtc_2 - gtc_1,9} ms : GetTickCount");

        Console.WriteLine();
    }
}
