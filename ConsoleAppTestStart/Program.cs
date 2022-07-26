using System;
using System.Diagnostics;

namespace ConsoleAppTestStart
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Console.WriteLine("Введите путь до:");
            string path_to_exe = Console.ReadLine();
            Console.WriteLine("Введите количество до:");
            int countProcess = Convert.ToInt16( Console.ReadLine() );
            Console.WriteLine("Начать ?:");
            Console.ReadLine();
            for (int i = 0; i < countProcess; i++)
            {
                Process process = new Process();
                process.StartInfo.FileName = path_to_exe;
                process.StartInfo.Arguments = "028991f1-7df6-4e20-9aef-280ae14f5a16";
                process.Start();
                //System.Diagnostics.Process.Start(path_to_exe, "028991f1-7df6-4e20-9aef-280ae14f5a16");
            }
            Console.WriteLine();
            Console.ReadLine();
        }
    }
}
