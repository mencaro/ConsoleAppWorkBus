using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.IO;

namespace ConsoleAppBus
{
    public class SettingsBase
    {
        private SettingsConnectionBus scb = null;
        public SettingsConnectionBus ConnectionBus
        {
            get { return scb; }
            set { scb = value; }
        }
        private SettingsQueueBus settingsQueueBus = null;
        public bool ValidateSettingsBus()
        {
            bool value = false;
            if (scb != null)
            {
                if (settingsQueueBus != null)
                {
                    value = true;
                }
            }
            return value;
        }
        public void GetSettings()
        {
            // string path = "/Users/eugene/Documents/content.txt";  // для MacOS/Linux
            GetSettingsConnection("Settings.json");
            GetSettingsQueue("SettingsQueue.json");
        }
        private void GetSettingsConnection(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
            {
                Console.WriteLine($"Имя файла: {fileInfo.Name}");
                Console.WriteLine($"Время создания: {fileInfo.CreationTime}");
                Console.WriteLine($"Размер: {fileInfo.Length}");

                using (StreamReader file = File.OpenText(path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    scb = (SettingsConnectionBus)serializer.Deserialize(file, typeof(SettingsConnectionBus));
                }
            }
            else
            {
                Console.WriteLine("File Settings not found");
            }
        }
        private void GetSettingsQueue(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
            {
                Console.WriteLine($"Имя файла: {fileInfo.Name}");
                Console.WriteLine($"Время создания: {fileInfo.CreationTime}");
                Console.WriteLine($"Размер: {fileInfo.Length}");

                using (StreamReader file = File.OpenText(path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    settingsQueueBus = (SettingsQueueBus)serializer.Deserialize(file, typeof(SettingsQueueBus));
                    Console.WriteLine(settingsQueueBus.ToBusString());
                }
            }
            else
            {
                Console.WriteLine("File Settings not found");
            }
        }

        public List<QueueBus> GetQueueBus()
        {
            return settingsQueueBus.sQueueBus;
        }
        public SettingsConnectionBus GetConnectiongSettings()
        {
            return scb;
        }
    }

    public class SettingsConnectionBus
    {
        public string _IP { get; set; }
        public int _Port { get; set; }
        public SettingsConnectionBus(string IP, int Port)
        {
            _IP = IP;
            _Port = Port;
        }
    }

    class SettingsQueueBus
    {
        public int Id { get; set; }
        public bool Active { get; set; }
        public List<QueueBus> sQueueBus { get; set; }

        public string ToBusString()
        {
            string value = "";

            foreach(QueueBus s in sQueueBus)
            {
                value += s._Id + " || " + s._NameQueue + " || " + s._DescriptionQueue + " || " + s._LengthQueue + " || ";
                value += "\n";
            }

            return value;
        }
        
    }

    public class QueueBus
    {
        public string _NameQueue { get; set; }
        public string _DescriptionQueue { get; set; }
        public int _LengthQueue { get; set; }
        public string _Id { get; set; }
        public bool _ActiveTick { get; set; }

        public bool _Durable { get; set; }// - если установлен - очередь существует и активна при перезагрузке сервера.Очередь может потерять сообщения посланные во время перезагрузки сервера
        public QueueBus()
        {

        }
        //Durable message queues - используются несколькими потребителями и существуют независимо от наличия потребителей которые могли бы принимать сообщения
        //Temporary message queues - приватные очереди для конкретного потребителя.Очередь удаляется при отсутствии потребителей.
    }
}
