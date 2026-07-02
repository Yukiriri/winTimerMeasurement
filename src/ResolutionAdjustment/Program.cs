using Microsoft.Win32;
using ResolutionAdjustment;

var windows_display_ver = "";
var windows_lcu_ver     = "";
{
    using var h_key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
    windows_display_ver = h_key?.GetValue("DisplayVersion")?.ToString() ?? "";
    windows_lcu_ver     = h_key?.GetValue("LCUVer")?.ToString() ?? "";
}

uint desired_res = 100;

Console.CursorVisible = false;
for (;; Thread.Sleep(0))
{
    Console.SetCursorPosition(0, 0);
    unsafe
    {
        Console.WriteLine($"""
                           Windows {windows_display_ver} {windows_lcu_ver}

                           """);

        uint actual_res = 0;
        WinNtApi.NtSetTimerResolution(desired_res, 1, &actual_res);
        Console.WriteLine($"""
                           NtSetTimerResolution :
                               desired : {desired_res * 100 / 1e+6,7:N3} ms
                               
                           """);

        uint min_res = 0, max_res = 0, cur_res = 0;
        WinNtApi.NtQueryTimerResolution(&max_res, &min_res, &cur_res);
        Console.WriteLine($"""
                           NtQueryTimerResolution :
                               min : {min_res * 100 / 1e+6,7:N3} ms 
                               max : {max_res * 100 / 1e+6,7:N3} ms
                               cur : {cur_res * 100 / 1e+6,7:N3} ms
                               
                           """);

        Console.WriteLine("""
                          ==============================
                          '-' : desired - 0.01ms
                          '+' : desired + 0.01ms

                          """);

        desired_res = Console.ReadKey().KeyChar switch
        {
            '-' => desired_res > 100 ? desired_res - 100 : desired_res,
            '+' => desired_res < max_res ? desired_res + 100 : desired_res,
            _   => desired_res
        };
    }
}
