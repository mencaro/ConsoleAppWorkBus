using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ConsoleAppProducerBus
{
    internal class ProduserBus
    {
        string _ADDRESS;
        int _PORT;
        string _IdMessage;
        string _Message;
        public ProduserBus(string IP, int Port)
        {
            _PORT = Port;
            _ADDRESS = IP;
        }

        public void SetMessage(string IdMessage, string Message)
        {
            _IdMessage = IdMessage;
            _Message = Message; 
        }
        public void Process()
        {
            TcpClient client = null;
            try
            {
                while (true)
                {
                    Console.Write("Начало отправки: ");
                    client = new TcpClient(_ADDRESS, _PORT);
                    NetworkStream stream = client.GetStream();
                    BinaryWriter writer = new BinaryWriter(stream);

                    writer.Write(_IdMessage);
                    writer.Write("PointToPoint");
                    writer.Write(_Message);
                    writer.Flush();

                    BinaryReader reader = new BinaryReader(stream);
                    string accountNumber = reader.ReadString();
                    Console.WriteLine("Номер вашего сообщения " + accountNumber);
                    
                    Console.Write("Отправленно: ");

                    reader.Close();
                    writer.Close();

                    stream.Close();
                    client.Close();

                    Thread.Sleep(5000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                client.Close();
            }
            Console.Read();
        }
    }
}
