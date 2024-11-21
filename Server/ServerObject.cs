using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Server
{
    public delegate void Notify();

    public class ServerObject
    {
        TcpListener tcpListener; // сервер для прослушивания
        List<ClientObject> clients = new List<ClientObject>(); // все подключения
        public event Notify ClientConnect;
        public event Notify ClientDisconnect;
        public event Notify ClientSendMessage;

        public ServerObject(IPAddress address, int port) 
        {
            tcpListener = new TcpListener(address, port);
        }
        protected internal void RemoveConnection(string id)
        {
            // получаем по id закрытое подключение
            ClientObject? client = clients.FirstOrDefault(c => c.Id == id);
            // и удаляем его из списка подключений
            if (client != null) clients.Remove(client);
            client?.Close();
        }
        // прослушивание входящих подключений
        protected internal async Task ListenAsync()
        {
            try
            {
                tcpListener.Start();
                
                while (true)
                {
                    TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();

                    ClientObject clientObject = new ClientObject(tcpClient, this);
                    clientObject.ClientExitChat += ClientExitFromChat;
                    clientObject.ClientSendMessage += ClientSendMSG;
                    clients.Add(clientObject);
                    ClientConnect.Invoke();
                    Task.Run(clientObject.ProcessAsync);
                }
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

        // трансляция сообщения подключенным клиентам
        protected internal async Task BroadcastMessageAsync(string message, string id)
        {
            foreach (var client in clients)
            {
                if (client.Id != id) // если id клиента не равно id отправителя
                {
                    await client.Writer.WriteLineAsync(message); //передача данных
                    await client.Writer.FlushAsync();
                }
            }
        }
        // отключение всех клиентов
        protected internal void Disconnect()
        {
            foreach (var client in clients)
            {
                client.Close(); //отключение клиента
            }
            tcpListener.Stop(); //остановка сервера
        }

        protected internal void ClientExitFromChat()
        {
            ClientDisconnect.Invoke();
        }
        protected internal void ClientSendMSG()
        {
            ClientSendMessage.Invoke();
        }

    }
}
