using System;
using System.Threading.Tasks;

namespace ConsoleAppProducerBus
{
    internal class Program
    {
        const int PORT = 8080;
        const string ADDRESS = "127.0.0.1";
        static string id;
        static string mess;
        static void Main(string[] args)
        {
            ProduserBus pb = new ProduserBus(ADDRESS, PORT);

            if (args.Length > 0)
            {
                Console.WriteLine(args[0].ToString());
                id = args[0];
                Random rnd = new Random();
                mess = rnd.Next().ToString();

                pb.SetMessage(id, mess);
            }
            else
            {
                pb.SetMessage("d6f7cdf4-97eb-46c2-9edd-8b9e468e4f43", "PointToPoint");
            }

            Task clientTask = new Task(pb.Process);
            clientTask.Start();

            Console.ReadLine();
        }
    }
}
