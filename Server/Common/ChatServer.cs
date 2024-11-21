using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Server.Common
{
    public delegate void Notify(int count);
    public delegate Task delMessageNotify(string message);
    public delegate void delCommandDeleteClientNotify(Guid guid);


    public class ChatServer
    {
        private TcpListener _listener;
        private List<ChatClient> _clients;
        private bool _isRunning;
        private int _countClientOnline;
        private int _countClientConnected;
        private int _countClientDisconnected;
        private int _countMessageSends;

        public event Notify? CountClientOnline;
        public event Notify? CountClientConnect;
        public event Notify? CountClientDisconnect;
        public event Notify? CountSendMessage;

        public ChatServer(IPAddress address, int port)
        {
            _listener = new TcpListener(address, port);
            _clients = new List<ChatClient>();

            _countClientConnected = 0;
            _countClientDisconnected = 0;
            _countMessageSends = 0;
        }

        public async Task ServerStartAndListenAsync()
        {
            try
            {
                _listener.Start();
                _isRunning = true;
                await AcceptChatClientAsync();
                
            }
            catch (Exception)
            {
                ServerStop();
            }
        }

        private async Task AcceptChatClientAsync()
        {
            while (_isRunning) 
            {
                TcpClient tcpClient = await _listener.AcceptTcpClientAsync();
                ChatClient client = new ChatClient(tcpClient);
                client.OnMessageNotify += SendMessageAsync;
                client.OnDeleteClientNotify += OnCommandDeleteClient;
                _clients.Add(client);
                Task.Run(client.ProcessAsync);
                _countClientConnected += 1;
                CountClientConnect?.Invoke(_countClientConnected);
                CountClientOnline?.Invoke(_clients.Count);
            }
        }

        protected internal void OnCommandDeleteClient(Guid id)
        {
            ChatClient? delClient = _clients.FirstOrDefault(x => x.Id == id);
            if (delClient != null)
            {
                delClient.Close();
                _clients.Remove(delClient);
                _countClientDisconnected += 1;
                CountClientDisconnect?.Invoke(_countClientDisconnected);
                CountClientOnline?.Invoke(_clients.Count);
            }

        }

        protected internal async Task SendMessageAsync(string message)
        {
            foreach (var client in _clients)
            {
                if(client._writer is not null)
                {
                    await client._writer.WriteLineAsync(message);
                    await client._writer.FlushAsync();
                } 
            }
            _countMessageSends += 1;
            CountSendMessage?.Invoke(_countMessageSends);
        }

        public void ServerStop()
        {
            foreach (var client in _clients) client.Close();
            _clients.Clear();
            _isRunning = false;
            _listener?.Stop();
        }
    }
}
