using ClassLibraryBusExpansion;
using System;
using System.Net.Sockets;
using System.Text.Json;
using System.Text;
using System.Threading;

namespace ConsoleAppTestClient
{
    internal class Program
    {
        const int PORT = 8080;
        const string ADDRESS = "127.0.0.1";
        static TcpClient client;
        static NetworkStream stream;

        public static int HandshakeMagic { get; private set; }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var rand = new Random();
            var t = new XPacketHandshake
            {
                MagicHandshakeNumber = rand.Next(100,200)
            };
            HandshakeMagic = t.MagicHandshakeNumber;
            var b = (XPacketConverter.Serialize((byte)XPacketType.Handshake,0,t).ToPacket());
            //
            client = new TcpClient();
            try
            {
                client.Connect(ADDRESS, PORT);  // подключение клиента
                stream = client.GetStream(); // получаем поток
                stream.Write(b, 0, b.Length);
                // запускаем новый поток для получения данных
                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start(); //старт потока

                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Disconnect();
            }
        }
        static void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    XPacketHandshake xp;
                    byte[] data = new byte[13]; // буфер для получаемых данных
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        var parsedPacket = XPacket.Parse(data);
                        ProcessIncomingPacket(parsedPacket);
                        xp = XPacketConverter.Deserialize<XPacketHandshake>(parsedPacket);
                    }
                    while (stream.DataAvailable);
                    
                    Console.WriteLine(xp.MagicHandshakeNumber.ToString());//вывод сообщения
                }
                catch
                {
                    Console.WriteLine("Подключение прервано!"); //соединение было прервано
                    Console.ReadLine();
                    Disconnect();
                }
            }
        }
        static void Disconnect()
        {
            if (stream != null)
                stream.Close();//отключение потока
            if (client != null)
                client.Close();//отключение клиента
            Environment.Exit(0); //завершение процесса
        }
        private static void ProcessIncomingPacket(XPacket packet)
        {
            var type = XPacketTypeManager.GetTypeFromPacket(packet);

            switch (type)
            {
                case XPacketType.Handshake:
                    ProcessHandshake(packet);
                    break;
                case XPacketType.Unknown:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private static void ProcessHandshake(XPacket packet)
        {
            var handshake = XPacketConverter.Deserialize<XPacketHandshake>(packet);

            if (HandshakeMagic - handshake.MagicHandshakeNumber == 15)
            {
                Console.WriteLine("Handshake successful!!!");
            }
        }
    }

    public class XPacketHandshake
    {
        [XField(1)]
        public int MagicHandshakeNumber;
    }
}
