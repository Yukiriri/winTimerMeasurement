using TimerResolution;

unsafe
{
    uint min_res = 0, max_res = 0, cur_res = 0;
    WinApi.NtQueryTimerResolution(&max_res, &min_res, &cur_res);
    Console.WriteLine($"""
                       NtQueryTimerResolution :
                            min : {min_res * 100 / 1e+6,7:N3} ms 
                            max : {max_res * 100 / 1e+6,7:N3} ms
                            cur : {cur_res * 100 / 1e+6,7:N3} ms
                            
                       """);

    uint desired_res = 10, actual_res = 0;
    WinApi.NtSetTimerResolution(desired_res, 1, &actual_res);
    Console.WriteLine($"""
                       NtSetTimerResolution :
                            desired : {desired_res * 100 / 1e+6,7:N3} ms
                            actual  : {actual_res * 100 / 1e+6,7:N3} ms
                            
                       """);
    
    WinApi.NtQueryTimerResolution(&max_res, &min_res, &cur_res);
    Console.WriteLine($"""
                       NtQueryTimerResolution :
                            cur : {cur_res * 100 / 1e+6,7:N3} ms
                            
                       """);
}
Console.WriteLine("Press any key to restore and exit...");
Console.ReadKey();
