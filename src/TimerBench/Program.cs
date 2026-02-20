using System.Text;
using Windows.Win32;
using Windows.Win32.Foundation;
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
    long qp_freq2 = 0;
    PInvoke.QueryPerformanceFrequency(&qp_freq2);
    qp_freq = qp_freq2;
}

Console.CursorVisible = false;
for (;; Thread.Sleep(1000 / 30))
{
    Console.SetCursorPosition(0, 0);

    Console.WriteLine(cpuid + "\n");
    
    Console.WriteLine($"{"WaitableTimer",15} : {"Request",12} | {"Actual",12}");
    testWaitableTimer(1000 / 30.0);
    testWaitableTimer(1000 / 60.0);
    testWaitableTimer(1);
    testWaitableTimer(0.5);
    testWaitableTimer(0.001);
}

unsafe void testWaitableTimer(double ms)
{
    const uint CREATE_WAITABLE_TIMER_HIGH_RESOLUTION = 0x2;
    const uint TIMER_ALL_ACCESS = 0x1F0003;

    var htimer = PInvoke.CreateWaitableTimerEx(null, null, CREATE_WAITABLE_TIMER_HIGH_RESOLUTION, TIMER_ALL_ACCESS);
    if (htimer != HANDLE.Null)
    {
        for (var i = DateTimeOffset.Now.ToUnixTimeMilliseconds(); DateTimeOffset.Now.ToUnixTimeMilliseconds() - i < 1000;)
        {
            var ns = (long)-(ms * 1e+6 / 100);
            if (PInvoke.SetWaitableTimer(htimer, &ns, 0, null, null, false))
            {
                long elapsed1 = 0, elapsed2 = 0;
                PInvoke.QueryPerformanceCounter(&elapsed1);
                if (PInvoke.WaitForSingleObject(htimer, uint.MaxValue) == 0)
                {
                    PInvoke.QueryPerformanceCounter(&elapsed2);
                    Console.Write($"{"",15}   {ms,9:N3} ms | {(elapsed2 - elapsed1) * 1e+3 / qp_freq,9:N3} ms \r");
                }
            }
        }
        Console.WriteLine();
        PInvoke.CloseHandle(htimer);
    }
}
