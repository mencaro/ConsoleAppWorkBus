using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ClassLibraryBusExpansion
{
    internal class MyProtocol
    {
    }
    /// <summary>
    /// типы пакетов
    /// </summary>
    public enum XPacketType
    {
        Unknown,//будет использоваться для типа, который нам неизвестен
        Handshake//для пакета рукопожатия
    }
    public class XPacket
    {
        /// <summary>
        /// тип пакета;
        /// </summary>
        public byte PacketType { get; private set; }
        /// <summary>
        /// подтип
        /// </summary>
        public byte PacketSubtype { get; private set; }
        /// <summary>
        /// набор полей.
        /// </summary>
        public List<XPacketField> Fields { get; set; } = new List<XPacketField>();

        private XPacket() { }

        public static XPacket Create(byte type, byte subtype)
        {
            return new XPacket
            {
                PacketType = type,
                PacketSubtype = subtype
            };
        }
        //запишем байты заголовка, типа и подтипа пакета, а потом отсортируем поля по возрастанию FieldID
        public byte[] ToPacket()
        {
            var packet = new MemoryStream();

            packet.Write(
            new byte[] { 0xAF, 0xAA, 0xAF, PacketType, PacketSubtype }, 0, 5);

            var fields = Fields.OrderBy(field => field.FieldID);

            foreach (var field in fields)
            {
                packet.Write(new[] { field.FieldID, field.FieldSize }, 0, 2);
                packet.Write(field.Contents, 0, field.Contents.Length);
            }

            packet.Write(new byte[] { 0xFF, 0x00 }, 0, 2);

            return packet.ToArray();
        }
        /// <summary>
        /// Проверяем размер входного пакета, его заголовок и два последних байта. После валидации пакета получим его тип и подтип
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public static XPacket Parse(byte[] packet)
        {
            if (packet.Length < 7)
            {
                return null;
            }

            if (packet[0] != 0xAF ||
                packet[1] != 0xAA ||
                packet[2] != 0xAF)
            {
                return null;
            }

            var mIndex = packet.Length - 1;

            if (packet[mIndex - 1] != 0xFF ||
                packet[mIndex] != 0x00)
            {
                return null;
            }

            var type = packet[3];
            var subtype = packet[4];

            var xpacket = Create(type, subtype);

            var fields = packet.Skip(5).ToArray();

            while (true)
            {
                if (fields.Length == 2)
                {
                    return xpacket;
                }

                var id = fields[0];
                var size = fields[1];

                var contents = size != 0 ?
                fields.Skip(2).Take(size).ToArray() : null;

                xpacket.Fields.Add(new XPacketField
                {
                    FieldID = id,
                    FieldSize = size,
                    Contents = contents
                });

                fields = fields.Skip(2 + size).ToArray();
            }
        }
        /// <summary>
        /// функцию для простого поиска поля по его ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public XPacketField GetField(byte id)
        {
            foreach (var field in Fields)
            {
                if (field.FieldID == id)
                {
                    return field;
                }
            }

            return null;
        }
        /// <summary>
        ///  функцию для проверки существования поля.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool HasField(byte id)
        {
            return GetField(id) != null;
        }
        /// <summary>
        /// Получаем значение из поля.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T GetValue<T>(byte id) where T : struct
        {
            var field = GetField(id);

            if (field == null)
            {
                throw new Exception($"Field with ID {id} wasn't found.");
            }
            var neededSize = Marshal.SizeOf(typeof(T));

            if (field.FieldSize != neededSize)
            {
                throw new Exception($"Can't convert field to type {typeof(T).FullName}.\n" + $"We have {field.FieldSize} bytes but we need exactly {neededSize}.");
            }

            return GeneralFunction.ByteArrayToFixedObject<T>(field.Contents);
        }
        /// <summary>
        /// Мы можем принять только объекты Value-Type. Они имеют фиксированный размер, поэтому мы можем их записать
        /// </summary>
        /// <param name="id"></param>
        /// <param name="structure"></param>
        /// <exception cref="Exception"></exception>
        public void SetValue(byte id, object structure)
        {
            if (!structure.GetType().IsValueType)
            {
                throw new Exception("Only value types are available.");
            }

            var field = GetField(id);

            if (field == null)
            {
                field = new XPacketField
                {
                    FieldID = id
                };

                Fields.Add(field);
            }

            var bytes = GeneralFunction.FixedObjectToByteArray(structure);

            if (bytes.Length > byte.MaxValue)
            {
                throw new Exception("Object is too big. Max length is 255 bytes.");
            }

            field.FieldSize = (byte)bytes.Length;
            field.Contents = bytes;
        }
    }

    /// <summary>
    /// класс для описания поля пакета, в котором будут его данные, ID и размер.
    /// </summary>
    public class XPacketField
    {
        public byte FieldID { get; set; }
        public byte FieldSize { get; set; }
        public byte[] Contents { get; set; }

    }
}
