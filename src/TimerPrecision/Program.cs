using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Windows.Win32;
using Microsoft.Win32;

Console.OutputEncoding = Encoding.UTF8;

string cpuid;
{
    using var hkey = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
    cpuid = hkey?.GetValue("ProcessorNameString").ToString() ?? "";
}

long qp_freq = 0;
unsafe
{
    PInvoke.QueryPerformanceFrequency(&qp_freq);
}

Console.CursorVisible = false;
for (;; Thread.Sleep(1000 / 30))
{
    Console.SetCursorPosition(0, 0);

    Console.WriteLine(cpuid + "\n");

    Console.WriteLine($"{qp_freq,9} hz : QueryPerformanceFrequency \n");

    unsafe
    {
        long qpc1 = 0, qpc2 = 0;
        PInvoke.QueryPerformanceCounter(&qpc1);
        while (qpc1 >= qpc2)
            PInvoke.QueryPerformanceCounter(&qpc2);
        Console.WriteLine($"{(qpc2 - qpc1) * 1e+9 / qp_freq,9} ns : QueryPerformanceCounter");

        ulong qitp1 = 0, qitp2 = 0;
        PInvoke.QueryInterruptTimePrecise(&qitp1);
        while (qitp1 >= qitp2)
            PInvoke.QueryInterruptTimePrecise(&qitp2);
        Console.WriteLine($"{(qitp2 - qitp1) * 100,9} ns : QueryInterruptTimePrecise");

        ulong quitp1 = 0, quitp2 = 0;
        PInvoke.QueryUnbiasedInterruptTimePrecise(&quitp1);
        while (quitp1 >= quitp2)
            PInvoke.QueryUnbiasedInterruptTimePrecise(&quitp2);
        Console.WriteLine($"{(quitp2 - quitp1) * 100,9} ns : QueryUnbiasedInterruptTimePrecise");

        ulong stp1 = 0, stp2 = 0;
        PInvoke.GetSystemTimePreciseAsFileTime((FILETIME*)&stp1);
        while (stp1 >= stp2)
            PInvoke.GetSystemTimePreciseAsFileTime((FILETIME*)&stp2);
        Console.WriteLine($"{(stp2 - stp1) * 100,9} ns : GetSystemTimePreciseAsFileTime");

        Console.WriteLine();

        ulong qit1 = 0, qit2 = 0;
        PInvoke.QueryInterruptTime(&qit1);
        while (qit1 >= qit2)
            PInvoke.QueryInterruptTime(&qit2);
        Console.WriteLine($"{(qit2 - qit1) * 100 / 1e+6,9:N3} ms : QueryInterruptTime");

        ulong quit1 = 0, quit2 = 0;
        PInvoke.QueryUnbiasedInterruptTime(&quit1);
        while (quit1 >= quit2)
            PInvoke.QueryUnbiasedInterruptTime(&quit2);
        Console.WriteLine($"{(quit2 - quit1) * 100 / 1e+6,9:N3} ms : QueryUnbiasedInterruptTime");

        ulong st1 = 0, st2 = 0;
        PInvoke.GetSystemTimeAsFileTime((FILETIME*)&st1);
        while (st1 >= st2)
            PInvoke.GetSystemTimeAsFileTime((FILETIME*)&st2);
        Console.WriteLine($"{(st2 - st1) * 100 / 1e+6,9:N3} ms : GetSystemTimeAsFileTime");

        Console.WriteLine();

        var gtc64_1 = PInvoke.GetTickCount64();
        var gtc64_2 = 0ul;
        while (gtc64_1 >= gtc64_2)
            gtc64_2 = PInvoke.GetTickCount64();
        Console.WriteLine($"{gtc64_2 - gtc64_1,9} ms : GetTickCount64");

        var gtc_1 = PInvoke.GetTickCount();
        var gtc_2 = 0ul;
        while (gtc_1 >= gtc_2)
            gtc_2 = PInvoke.GetTickCount();
        Console.WriteLine($"{gtc_2 - gtc_1,9} ms : GetTickCount");

        Console.WriteLine();
    }
}
