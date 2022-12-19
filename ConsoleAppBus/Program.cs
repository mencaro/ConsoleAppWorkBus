
using System;

namespace ConsoleAppBus
{
    internal class Program
    {
        static Bus bus = null;
        static void Main(string[] args)
        {
            Console.WriteLine("Start Bus Work!\n");
            //--------------
            bus = new Bus();
            //-----------------
            Console.ReadLine();
        }
    }

    
}
