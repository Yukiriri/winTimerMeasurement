using System.Text;
using Windows.Win32;

Console.OutputEncoding = Encoding.UTF8;

PInvoke.timeBeginPeriod(1);
Console.WriteLine("timeBeginPeriod = 1ms");

Console.WriteLine("Press any key to restore and exit...");
Console.ReadKey();

PInvoke.timeEndPeriod(1);
