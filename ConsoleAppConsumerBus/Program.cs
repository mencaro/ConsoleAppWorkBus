using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleAppConsumerBus
{
    internal class Program
    {
        const int PORT = 8080;
        const string ADDRESS = "127.0.0.1";
        static TcpClient client;
        static NetworkStream stream;

        static void Main(string[] args)
        {
            //Console.WriteLine("Введите индетификатор очереди на которую следует подписаться: ");
            //string id_queue = Console.ReadLine();
            //ConsumerBus сb = new ConsumerBus(ADDRESS, PORT, id_queue);
            //Task TaskSubscrube = new Task(сb.Process);
            //TaskSubscrube.Start();
            //Thread.Sleep(5000);
            //Console.WriteLine("Начать прием сообщений: ");
            //Task TaskMessage = new Task(сb.GetMessageFromQueue);
            //TaskMessage.Start();
            //Console.WriteLine("Читаем: ");
            //Console.ReadLine();
            ////=======================
            client = new TcpClient();
            try
            {
                client.Connect(ADDRESS, PORT); //подключение клиента
                //stream = client.GetStream(); // получаем поток

                Console.WriteLine("Введите индетификатор очереди на которую следует подписаться: ");
                string id_queue = Console.ReadLine();
                ConsumerBus сb = new ConsumerBus(client, ADDRESS, PORT, id_queue);
                сb.Process();
                //string message = userName;
                //byte[] data = Encoding.Unicode.GetBytes(message);
                //stream.Write(data, 0, data.Length);

                Thread.Sleep(1000);
                // запускаем новый поток для получения данных
                Task TaskMessage = new Task(сb.GetMessageFromQueue);
                TaskMessage.Start();
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Disconnect();
            }
        }
        static void Disconnect()
        {
            if (stream != null)
                stream.Close();//отключение потока
            if (client != null)
                client.Close();//отключение клиента
            Environment.Exit(0); //завершение процесса
        }
    }

    internal class ConsumerBus
    {
        string _ADDRESS;
        int _PORT;
        string _id_queue;
        string _IdMessage;
        string _Message;
        static NetworkStream stream;
        TcpClient _client;
        Guid _id;
        public ConsumerBus(TcpClient client, string IP, int Port, string id_queue)
        {
            _PORT = Port;
            _ADDRESS = IP;
            _id_queue = id_queue;
            _client = client;
            _id = Guid.NewGuid();
        }

        public void SetMessage(string IdMessage, string Message)
        {
            _IdMessage = IdMessage;
            _Message = Message;
        }
        public void Process()
        {
            try
            {
                if (_client != null)
                {
                    Console.Write("Начало подписки: ");
                    stream = _client.GetStream();

                    BinaryWriter writer = new BinaryWriter(stream);

                    writer.Write(_id_queue);
                    writer.Write("Subscription");
                    writer.Write("Подписка");
                    writer.Flush();

                    BinaryReader reader = new BinaryReader(stream);
                    string accountNumber = reader.ReadString();
                    Console.WriteLine("Номер вашего сообщения о подписке" + accountNumber);

                    //stream.Close();
                    //reader.Close();
                    //writer.Close();
                    Console.WriteLine("Подписан: ");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                //client.Close();
            }
            //Console.Read();
        }
        public void GetMessageFromQueue()
        {
            try
            {
                while (true)
                {
                    //Console.WriteLine("Читаем!");
                    try
                    {
                        byte[] data = new byte[64]; // буфер для получаемых данных
                        StringBuilder builder = new StringBuilder();
                        int bytes = 0;
                        do
                        {
                            bytes = stream.Read(data, 0, data.Length);
                            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        }
                        while (stream.DataAvailable);
                        if (builder.ToString() != "")
                        {
                            string message = builder.ToString();
                            Console.WriteLine(message);//вывод сообщения
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Подключение прервано!"); //соединение было прервано
                        Console.ReadLine();
                        Disconnect();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _client.Close();
            }
            Console.Read();
        }
        void Disconnect()
        {
            if (stream != null)
                stream.Close();//отключение потока
            if (_client != null)
                _client.Close();//отключение клиента
            Environment.Exit(0); //завершение процесса
        }
    }
}
