using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ClassLibraryBusExpansion
{
    //структуру пакетов для их сериализации и десериализации
    [AttributeUsage(AttributeTargets.Field)]//Используя AttributeUsage, мы установили, что наш атрибут можно будет установить только на поля классов. FieldID будет использоваться для хранения ID поля внутри пакета.
    public class XFieldAttribute : Attribute
    {
        public byte FieldID { get; }

        public XFieldAttribute(byte fieldId)
        {
            FieldID = fieldId;
        }
    }
    public static class XPacketConverter
    { 
        //собрать информацию о полях, которые будут участвовать в процессе сериализации. Для этого можно использовать простое выражение LINQ
        private static List<Tuple<FieldInfo, byte>> GetFields(Type t)
        {
            return t.GetFields(BindingFlags.Instance |
                               BindingFlags.NonPublic |
                               BindingFlags.Public)
            .Where(field => field.GetCustomAttribute<XFieldAttribute>() != null)
            .Select(field => Tuple.Create(field, field.GetCustomAttribute<XFieldAttribute>().FieldID))
            .ToList();
        }
        /// <summary>
        ///  сам сериализатор:
        /// </summary>
        /// <param name="type"></param>
        /// <param name="subtype"></param>
        /// <param name="obj"></param>
        /// <param name="strict"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static XPacket Serialize(byte type, byte subtype, object obj, bool strict = false)
        {
            var fields = GetFields(obj.GetType());

            if (strict)
            {
                var usedUp = new List<byte>();

                foreach (var field in fields)
                {
                    if (usedUp.Contains(field.Item2))
                    {
                        throw new Exception("One field used two times.");
                    }

                    usedUp.Add(field.Item2);
                }
            }

            var packet = XPacket.Create(type, subtype);

            foreach (var field in fields)
            {
                packet.SetValue(field.Item2, field.Item1.GetValue(obj));
            }

            return packet;
        }
        /// <summary>
        /// десериализация
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="packet"></param>
        /// <param name="strict"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static T Deserialize<T>(XPacket packet, bool strict = false)
        {
            var fields = GetFields(typeof(T));
            var instance = Activator.CreateInstance<T>();

            if (fields.Count == 0)
            {
                return instance;
            }

            /* <---> */
            foreach (var tuple in fields)
            {
                var field = tuple.Item1;
                var packetFieldId = tuple.Item2;

                if (!packet.HasField(packetFieldId))
                {
                    if (strict)
                    {
                        throw new Exception($"Couldn't get field[{packetFieldId}] for {field.Name}");
                    }

                    continue;
                }

                /* Очень важный костыль, который многое упрощает
                 * Метод GetValue<T>(byte) принимает тип как type-параметр
                 * Наш же тип внутри field.FieldType
                 * Используя Reflection, вызываем метод с нужным type-параметром
                 */

                var value = typeof(XPacket)
                    .GetMethod("GetValue")?
                    .MakeGenericMethod(field.FieldType)
                    .Invoke(packet, new object[] { packetFieldId });

                if (value == null)
                {
                    if (strict)
                    {
                        throw new Exception($"Couldn't get value for field[{packetFieldId}] for {field.Name}");
                    }

                    continue;
                }

                field.SetValue(instance, value);
            }

            return instance;
        }
    }
}
