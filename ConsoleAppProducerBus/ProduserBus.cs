using ClassLibraryBusExpansion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace ConsoleAppProducerBus
{
    internal class ProduserBus
    {
        string _ADDRESS;
        int _PORT;
        string _IdMessage;
        string _Message;
        NetworkStream stream;
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
            client = new TcpClient();
            Guid _guid = Guid.NewGuid();
            _guid = Guid.NewGuid();
            try
            {
                client.Connect(_ADDRESS, _PORT);  // подключение клиента
                stream = client.GetStream(); // получаем поток
                MessageGateway mesObj = new MessageGateway(_guid.ToString(), "d6f7cdf4-97eb-46c2-9edd-8b9e468e4f43", "PointToPoint", "123456789");
                while (true)
                {
                    Console.Write("Начало отправки: ");
                    //=================================
                    string message = JsonSerializer.Serialize(mesObj);
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                    //=================================
                    Thread.Sleep(1000);
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
