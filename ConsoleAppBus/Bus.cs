using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleAppBus
{
    public class Bus
    {
        /// <summary>
        /// потока для прослушивания шлюза
        /// </summary>
        static Thread listenThreadGateway;
        /// <summary>
        /// Настройки
        /// </summary>
        private SettingsBase _settingsBase = null;
        /// <summary>
        /// Шлюз
        /// </summary>
        private Gateway gateway = null;
        /// <summary>
        /// Очереди на шине
        /// </summary>
        System.Collections.Concurrent.ConcurrentDictionary<string, ElementQueue> BusQuueues = null;
        public Bus()
        {
            _settingsBase = new SettingsBase();
            SettingsBase.GetSettings();
            if (ValidateSettingsBus())
            {
                Console.WriteLine("settings On");
                CreateQueue();
                StartGateway();
            }
            else
            {
                Console.WriteLine("settings Off");
                Console.WriteLine("bus Stop");
            }
        }

        public SettingsBase SettingsBase
        {
            get
            {
                return _settingsBase;
            }
            set
            {
                _settingsBase = value;
            }
        }
        public bool ValidateSettingsBus()
        {
            bool value = false;
            if (SettingsBase != null)
            {
                if (SettingsBase.ValidateSettingsBus())
                {
                    value = true;
                }
            }
            return value;
        }
        private void StartGateway()
        {
            gateway = new Gateway(this);
            gateway.scb = SettingsBase.GetConnectiongSettings();
            listenThreadGateway = new Thread(new ThreadStart(gateway.Listen));
            listenThreadGateway.Start(); //старт потока
        }
        private void CreateQueue()
        {
            if (SettingsBase != null)
            {
                BusQuueues = new System.Collections.Concurrent.ConcurrentDictionary<string, ElementQueue>();
                List<QueueBus> queueBuses = SettingsBase.GetQueueBus();
                foreach(QueueBus queueBus in queueBuses)
                {
                    BusQuueues.TryAdd(queueBus._Id, new ElementQueue(queueBus));
                }
            }
        }
        private void StartBus()
        {

        }

        public void AddMessageToQueue(MessageGateway messageGateway)
        {
            if (messageGateway != null)
            {
                if (BusQuueues != null)
                {
                    BusQuueues.TryGetValue(messageGateway.IdQueueMessage, out var elementQueue);
                    if (elementQueue != null)
                    {
                        elementQueue.AddMessageToQueue(messageGateway);
                    }
                }
            }
        }
        public void AddGatewayClientObjectToQueue(GatewayClientObjectBinary gcob)
        {
            if (gcob != null)
            {
                if (BusQuueues != null)
                {
                    BusQuueues.TryGetValue(gcob.GetMessageGateway().IdQueueMessage, out var elementQueue);
                    if (elementQueue != null)
                    {
                        elementQueue.AddGatewayClientObjectToQueue(gcob);
                    }
                }
            }
        }

        //public void RoutingMessageGatewayClientObjectToBus(GatewayClientObjectBinary gcob)
        //{
        //    if (gcob.GetMessageGateway() == null)
        //    {
        //        if (gcob.GetMessageGateway().TypeMessage == ClassLibraryBusExpansion.TypeBusMessage.PointToPoint)
        //        {
        //            AddMessageToQueue(gcob.GetMessageGateway());
        //        }
        //        else if (gcob.GetMessageGateway().TypeMessage == ClassLibraryBusExpansion.TypeBusMessage.Subscription)
        //        {
        //            AddGatewayClientObjectToQueue(gcob);
        //        }
        //        else if (gcob.GetMessageGateway().TypeMessage == ClassLibraryBusExpansion.TypeBusMessage.RequestResponse)
        //        {

        //        }
        //    }
        //}
        
    }

    public class ElementQueue
    {
        /// <summary>
        /// Настройки очереди
        /// </summary>
        private QueueBus _settingsQueue;
        /// <summary>
        /// Очередь сообщений в очереди
        /// </summary>
        private System.Collections.Concurrent.ConcurrentQueue<MessageGateway> QueueMessageInElementQueue = null;
        /// <summary>
        /// Подписки клиентов на очередь в очередь получения соощений
        /// </summary>
        private List<GatewayClientObjectBinary> SubscriptionClient = null;
        //===============================================================
        public ElementQueue(QueueBus settingsQueue)
        {
            _settingsQueue = settingsQueue;
            QueueMessageInElementQueue = new System.Collections.Concurrent.ConcurrentQueue<MessageGateway>();
            SubscriptionClient = new List<GatewayClientObjectBinary>();
            //
            StartTickQueue();
        }

        public void AddMessageToQueue(MessageGateway messageGateway)
        {
            QueueMessageInElementQueue.Enqueue(messageGateway);
            Console.WriteLine("Добавлено в очередь... " + _settingsQueue._NameQueue);
        }
        public void AddGatewayClientObjectToQueue(GatewayClientObjectBinary gcob)
        {
            Console.WriteLine("Внесение подписчика: ");
            SubscriptionClient.Add(gcob);
        }

        private void StartTickQueue()
        {
            Task TickQueue = new Task(TickWorkElementQueue);
            Console.WriteLine("Старт очереди: " + _settingsQueue._NameQueue);
            TickQueue.Start();
        }
        private void TickWorkElementQueue()
        {
            while (true)
            {
                WorkElementQueue();
                Thread.Sleep(10);
            }
        }
        private void WorkElementQueue()
        {
            if (SubscriptionClient != null)
            {
                if (QueueMessageInElementQueue != null)
                {
                    SetMessageToSubscriptionClient();
                }
            }
        }

        private void SetMessageToSubscriptionClient()
        {
            if ((SubscriptionClient.Count > 0))
            {
                if ((QueueMessageInElementQueue.Count > 0))
                {
                    Console.WriteLine("Сообщения есть: ");
                    if (QueueMessageInElementQueue.TryDequeue(out var mes))
                    {
                        Console.WriteLine("Отправление сообщения подписчикам: " + mes.IdGenerateGuid);
                        for (int i = 0; i < SubscriptionClient.Count; i++)
                        {
                            SubscriptionClient[i].PushMessage(mes);
                        }
                    }
                }
            }
        }
    }
}
