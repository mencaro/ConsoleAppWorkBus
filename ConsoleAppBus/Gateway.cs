using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ClassLibraryBusExpansion;

namespace ConsoleAppBus
{
    /// <summary>
    /// Шлюз
    /// </summary>
    internal class Gateway
    {
        static TcpListener tcpListener; // сервер для прослушивания

        private Bus _OuterBus = null;
        public SettingsConnectionBus scb { get; set; }

        public Gateway(Bus OuterBus)
        {
            _OuterBus = OuterBus;
        }
        public void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Parse(_OuterBus.SettingsBase.ConnectionBus._IP), _OuterBus.SettingsBase.ConnectionBus._Port);
                tcpListener.Start();
                Console.WriteLine("Шлюз запущен. Ожидание подключений...");

                while (true)
                {
                    TcpClient client = tcpListener.AcceptTcpClient();
                    GatewayClientObjectBinary clientObject = new GatewayClientObjectBinary(client, _OuterBus);
                    Console.WriteLine("Новое соединение...");
                    // создаем новый поток для обслуживания нового клиента
                    Task clientTask = new Task(clientObject.Process);
                    clientTask.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (tcpListener != null)
                    tcpListener.Stop();
            }
        }
    }

    public class GatewayClientObjectBinary
    {
        public TcpClient client;
        private Bus _OuterBus = null;
        private MessageGateway _messageGateway = null;
        NetworkStream stream = null;
        public GatewayClientObjectBinary(TcpClient tcpClient, Bus outerBus)
        {
            client = tcpClient;
            _OuterBus = outerBus;
        }

        public void Process()
        {
            try
            {
                stream = client.GetStream();
                BinaryReader reader = new BinaryReader(stream);
                //=============================================
                // считываем данные из потока
                string IdMessage = reader.ReadString();
                string TypeMessage = reader.ReadString();
                string IdJSONmarkup = reader.ReadString();
                //========================================
                // создаем по полученным от клиента данным объект счета
                _messageGateway = new MessageGateway(IdMessage, TypeMessage, IdJSONmarkup);
                //========================================================================================
                Console.WriteLine("{0} зарегистрировано сообщение: {1}, типа {2}", _messageGateway.IdQueueMessage, _messageGateway.IdTailMessage, _messageGateway.TypeMessage);
                //=============================================================================================================================================================
                RoutingMessageGatewayClientObjectToBus();
                // отправляем ответ в виде номера счета
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write(_messageGateway.IdGenerateGuid);
                writer.Flush();
                //=============
                //stream.Close();
                //writer.Close();
                //reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (GetMessageGateway().TypeMessage != ClassLibraryBusExpansion.Routing_Key.Subscription)
                {
                    if (stream != null)
                        stream.Close();
                
                    if (client != null)
                        client.Close();
                }
                else
                {
                    Console.WriteLine("Клиент сохранен");
                }
            }
        }

        private void RoutingMessageGatewayClientObjectToBus()
        {
            if (GetMessageGateway() != null)
            {
                if (GetMessageGateway().TypeMessage == ClassLibraryBusExpansion.Routing_Key.PointToPoint)
                {
                    _OuterBus.AddMessageToQueue(GetMessageGateway());
                }
                else if (GetMessageGateway().TypeMessage == ClassLibraryBusExpansion.Routing_Key.Subscription)
                {
                    Console.WriteLine("Начало подписки: ");
                    _OuterBus.AddGatewayClientObjectToQueue(this);
                }
                else if (GetMessageGateway().TypeMessage == ClassLibraryBusExpansion.Routing_Key.RequestResponse)
                {

                }
            }
        }
        public void PushMessage(MessageGateway messageFromQueue)
        {
            var TickPush = new Task(new Action<object>(Push), messageFromQueue);
            TickPush.Start();
        }
        private void Push(object mes)
        {
            stream = client.GetStream();

            string message = ((MessageGateway)mes).IdTailMessage;
            byte[] data = Encoding.Unicode.GetBytes(message);
            stream.Write(data, 0, data.Length);

            Console.Write("Отправленно подписчику: ");
        }

        public MessageGateway GetMessageGateway()
        {
            return _messageGateway;
        }
    }

    public class MessageGateway
    {
        /// <summary>
        /// сгенерированный гуид при регистрации сообщения по прибытию на шину
        /// </summary>
        public string IdGenerateGuid { get; set; }
        /// <summary>
        /// ID очереди, в которую следует поместить сообщение
        /// </summary>
        public string IdQueueMessage { get; set; }
        /// <summary>
        /// Тип сообщения/маршрутизации
        /// </summary>
        public Routing_Key TypeMessage { get; set; }
        /// <summary>
        /// Тело сообщения
        /// </summary>
        public string IdTailMessage { get; set; }
        public MessageGateway(string idQueueMessage, string typeMessage, string idTailMessage)
        {
            IdQueueMessage = idQueueMessage;
            IdTailMessage = idTailMessage;
            GenerateGuidForMessage();
            GenerateTypeForMessage(typeMessage);
        }

        private void GenerateGuidForMessage()
        {
            Guid g = new Guid();
            g = Guid.NewGuid();
            IdGenerateGuid = g.ToString();
        }
        private void GenerateTypeForMessage(string type)
        {
            TypeMessage = (Routing_Key)Enum.Parse(typeof(Routing_Key), type, true);
        }
    }

}
