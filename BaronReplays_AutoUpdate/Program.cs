using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace BaronReplays_AutoUpdate
{
    class Program
    {
        static void Main(string[] args)
        {
            Process BR = CheckProcessIsAlive("BaronReplays");
            if (BR == null)
                System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"\BaronReplays.exe");
            else
            {
                BR.Kill();
                do
                {
                    Thread.Sleep(100);
                }
                while (CheckProcessIsAlive("BaronReplays") != null);
                Console.WriteLine("BaronReplays is updating...");
                System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"\BaronReplays_Auto.exe");
                do
                {
                    Thread.Sleep(100);
                }
                while (CheckProcessIsAlive("BaronReplays_Auto") != null);
                System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"\BaronReplays.exe");
            }
        }

        private static Process CheckProcessIsAlive(String name)
        {

            Process[] detectBR = System.Diagnostics.Process.GetProcessesByName(name);
            if (detectBR.Length == 0)
                return null;
            return detectBR[0];
        }
    }
}
