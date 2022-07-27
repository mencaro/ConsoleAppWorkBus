using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ClassLibraryBusExpansion;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;

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

        List<GatewayClientObjectBinary> clients = new List<GatewayClientObjectBinary>(); // все подключения

        protected internal void AddConnection(GatewayClientObjectBinary clientObject)
        {
            clients.Add(clientObject);
        }

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
                    Console.WriteLine("Новое соединение...");
                    GatewayClientObjectBinary clientObject = new GatewayClientObjectBinary(client, _OuterBus);
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
        private MessageGateway _messageGateway2 = null;
        protected internal NetworkStream Stream { get; private set; }
        public GatewayClientObjectBinary(TcpClient tcpClient, Bus outerBus)
        {
            client = tcpClient;
            _OuterBus = outerBus;
        }
        public void ProcessListen()
        {
            try
            {
                while(true)
                {
                    _messageGateway2 = _OuterBus.SettingsBase.GetMessageQueue(GetMessage());
                    _OuterBus.AddMessageToQueue(GetMessageGateway2());
                    BroadcastMessage(JsonConvert.SerializeObject(_messageGateway2));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if ((GetMessageGateway().TypeMessage != ClassLibraryBusExpansion.Routing_Key.Subscription) || (GetMessageGateway().TypeMessage != ClassLibraryBusExpansion.Routing_Key.PointToPoint))
                {
                    if (Stream != null)
                        Stream.Close();

                    if (client != null)
                        client.Close();
                }
            }
        }
        public void Process()
        {
            try
            {
                Stream = client.GetStream();
                //==========================
                //====================================================
                // создаем по полученным от клиента данным объект счета
                string s1 = GetMessage();
                _messageGateway = _OuterBus.SettingsBase.GetMessageQueue(s1);
                //========================================================================================
                Console.WriteLine("{0} зарегистрировано сообщение: {1}, типа {2}", _messageGateway.QueueMessage, _messageGateway.TailMessage, _messageGateway.TypeMessage);
                //=========================================================================================================================================================
                //BroadcastMessage(JsonConvert.SerializeObject(_messageGateway));
                RoutingMessageGatewayClientObjectToBus();

                while (true)
                {
                    string s2 = GetMessage();
                    _messageGateway = _OuterBus.SettingsBase.GetMessageQueue(s2);
                    RoutingMessageGatewayClientObjectToBus();
                    //BroadcastMessage(JsonConvert.SerializeObject(_messageGateway2));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if ((GetMessageGateway().TypeMessage != ClassLibraryBusExpansion.Routing_Key.Subscription) || (GetMessageGateway().TypeMessage != ClassLibraryBusExpansion.Routing_Key.PointToPoint))
                {
                    if (Stream != null)
                        Stream.Close();
                
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
                    Console.WriteLine("Регистрация Produser: ");
                    _OuterBus.AddGatewayClientObjectBinaryToDic(this);
                    //_OuterBus.AddMessageToQueue(GetMessageGateway());
                }
                else if (GetMessageGateway().TypeMessage == ClassLibraryBusExpansion.Routing_Key.Subscription)
                {
                    Console.WriteLine("Регистрация Consumer: ");
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
            Stream = client.GetStream();

            string message = ((MessageGateway)mes).TailMessage;
            byte[] data = Encoding.Unicode.GetBytes(message);
            Stream.Write(data, 0, data.Length);

            Console.Write("Отправленно подписчику: ");
        }

        public MessageGateway GetMessageGateway()
        {
            return _messageGateway;
        }
        public MessageGateway GetMessageGateway2()
        {
            return _messageGateway2;
        }
        //
        // чтение входящего сообщения и преобразование в строку
        private string GetMessage()
        {
            byte[] data = new byte[64]; // буфер для получаемых данных
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return builder.ToString();
        }

        // трансляция сообщения подключенным клиентам
        protected internal void BroadcastMessage(string message)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            Stream.Write(data, 0, data.Length); //передача данных
        }
        // отключение всех клиентов
        public void Disconnect()
        {
            //tcpListener.Stop(); //остановка сервера
        }
    }
    //[Serializable]
    //public class MessageGateway
    //{
    //    /// <summary>
    //    /// сгенерированный гуид при регистрации сообщения по прибытию на шину
    //    /// </summary>
    //    public Guid GenerateGuid { get; set; }

    //    /// <summary>
    //    /// ID очереди, в которую следует поместить сообщение
    //    /// </summary>
    //    public string QueueMessage { get; set; }

    //    /// <summary>
    //    /// Тип сообщения/маршрутизации
    //    /// </summary>
    //    public Routing_Key TypeMessage { get; set; }

    //    /// <summary>
    //    /// Тело сообщения
    //    /// </summary>
    //    public string TailMessage { get; set; }

    //    public MessageGateway(string idQueueMessage, string typeMessage, string idTailMessage)
    //    {
    //        QueueMessage = idQueueMessage;
    //        TailMessage = idTailMessage;

    //        //Guid g = new Guid();
    //        GenerateGuid = Guid.NewGuid();

    //        GenerateTypeForMessage(typeMessage);
    //    }
        
    //    private string GetSGenerateGuidForMessage()
    //    {
    //        return GenerateGuid.ToString();
    //    }
    //    private void GenerateTypeForMessage(string type)
    //    {
    //        TypeMessage = (Routing_Key)Enum.Parse(typeof(Routing_Key), type, true);
    //    }
    //}

}
