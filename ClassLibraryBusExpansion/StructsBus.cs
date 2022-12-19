using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibraryBusExpansion
{
    internal class StructsBus
    {
    }

    public enum Routing_Key
    {
        PointToPoint,   // точка - точка
        Subscription,   // подписка
        RequestResponse // запрос - ответ
    }
    //Fanout;Direct;Topic;Headers.

    [Serializable]
    public class MessageGateway
    {
        public string _idProducer { get; set; }
        /// <summary>
        /// сгенерированный гуид при регистрации сообщения по прибытию на шину
        /// </summary>
        public Guid GenerateGuid { get; set; }

        /// <summary>
        /// ID очереди, в которую следует поместить сообщение
        /// </summary>
        public string QueueMessage { get; set; }

        /// <summary>
        /// Тип сообщения/маршрутизации
        /// </summary>
        public Routing_Key TypeMessage { get; set; }

        /// <summary>
        /// Тело сообщения
        /// </summary>
        public string TailMessage { get; set; }

        public MessageGateway(string idProducer, string idQueueMessage, string typeMessage, string idTailMessage)
        {
            _idProducer = idProducer;
            QueueMessage = idQueueMessage;
            TailMessage = idTailMessage;
            GenerateGuid = Guid.NewGuid();
            GenerateTypeForMessage(typeMessage);
        }
        private void GenerateTypeForMessage(string type)
        {
            TypeMessage = (Routing_Key)Enum.Parse(typeof(Routing_Key), type, true);
        }
    }
}
