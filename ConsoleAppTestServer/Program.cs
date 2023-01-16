using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ClassLibraryBusExpansion;

namespace ConsoleServer
{
    class Program
    {
        const int port = 8080;
        static TcpListener listener;
        static void Main(string[] args)
        {
            try
            {
                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
                listener.Start();
                Console.WriteLine("Ожидание подключений...");

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    ClientObjectBase clientObject = new ClientObjectBase(client);

                    // создаем новый поток для обслуживания нового клиента
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (listener != null)
                    listener.Stop();
            }
        }
    }

    public class XPacketHandshake
    {
        [XField(1)]
        public int MagicHandshakeNumber;
    }
    public class ClientObjectBase
    {
        public TcpClient client;
        public ClientObjectBase(TcpClient tcpClient)
        {
            client = tcpClient;
        }

        public void Process()
        {
            NetworkStream stream = null;
            try
            {
                stream = client.GetStream();
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
                            //ProcessHandshake(parsedPacket);
                            xp = XPacketConverter.Deserialize<XPacketHandshake>(parsedPacket);
                        }
                        while (stream.DataAvailable);

                        Console.WriteLine(xp.MagicHandshakeNumber.ToString());//вывод сообщения
                        xp.MagicHandshakeNumber += 100;
                        data = (XPacketConverter.Serialize((byte)XPacketType.Handshake, 0, xp).ToPacket());
                        stream.Write(data, 0, data.Length);
                    }
                    catch
                    {
                        Console.WriteLine("Подключение прервано!"); //соединение было прервано
                        Console.ReadLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (stream != null)
                    stream.Close();
                if (client != null)
                    client.Close();
            }
        }

        private void ProcessHandshake(XPacket packet)
        {
            Console.WriteLine("Recieved handshake packet.");

            var handshake = XPacketConverter.Deserialize<XPacketHandshake>(packet);
            handshake.MagicHandshakeNumber -= 15;

            Console.WriteLine("Answering..");
        }
    }
}