using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibraryBusExpansion
{
    internal class Managers
    {
    }
    //Теперь, когда нам известны типы пакетов, пора привязать их к ID. Необходимо создать менеджер, который будет этим заниматься.
    public static class XPacketTypeManager
    {
        private static readonly Dictionary<XPacketType, Tuple<byte, byte>> TypeDictionary = new Dictionary<XPacketType, Tuple<byte, byte>>();
        /* < ... > */
        /// <summary>
        /// функцию для регистрации типов пакета
        /// </summary>
        /// <param name="type"></param>
        /// <param name="btype"></param>
        /// <param name="bsubtype"></param>
        /// <exception cref="Exception"></exception>
        public static void RegisterType(XPacketType type, byte btype, byte bsubtype)
        {
            if (TypeDictionary.ContainsKey(type))
            {
                throw new Exception($"Packet type {type:G} is already registered.");
            }

            TypeDictionary.Add(type, Tuple.Create(btype, bsubtype));
        }
        /// <summary>
        /// Имплементируем получение информации по типу:
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Tuple<byte, byte> GetType(XPacketType type)
        {
            if (!TypeDictionary.ContainsKey(type))
            {
                throw new Exception($"Packet type {type:G} is not registered.");
            }

            return TypeDictionary[type];
        }
        /// <summary>
        /// получение типа пакета
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public static XPacketType GetTypeFromPacket(XPacket packet)
        {
            var type = packet.PacketType;
            var subtype = packet.PacketSubtype;

            foreach (var tuple in TypeDictionary)
            {
                var value = tuple.Value;

                if (value.Item1 == type && value.Item2 == subtype)
                {
                    return tuple.Key;
                }
            }

            return XPacketType.Unknown;
        }
    }
}
