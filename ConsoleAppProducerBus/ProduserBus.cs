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
        string _GuidQueue;
        string _TypeMessage;
        string _TileMess;
        NetworkStream stream;
        public ProduserBus(string IP, int Port)
        {
            _PORT = Port;
            _ADDRESS = IP;
        }

        public void SetMessage(string GuidQueue, string TypeMessage)
        {
            _GuidQueue = GuidQueue;
            _TypeMessage = TypeMessage; 
        }
        public void Process()
        {
            Random rnd = new Random();
            TcpClient client = null;
            client = new TcpClient();
            Guid _guid = Guid.NewGuid();
            _guid = Guid.NewGuid();
            try
            {
                client.Connect(_ADDRESS, _PORT);  // подключение клиента
                stream = client.GetStream(); // получаем поток
                _TileMess = "123456789";
                MessageGateway mesObj = new MessageGateway(_guid.ToString(), _GuidQueue, _TypeMessage, _TileMess);
                while (true)
                {
                    Console.Write("Начало отправки: ");
                    //=================================
                    string message = JsonSerializer.Serialize(mesObj);
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                    //=================================
                    Thread.Sleep(rnd.Next(10,1000));
                    mesObj.TailMessage = rnd.Next().ToString();
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
