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
        PointToPoint,// точка-точка
        Subscription,//подписка
        RequestResponse//запрос - ответ
    }
}
