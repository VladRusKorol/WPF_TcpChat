using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Client
{
    public delegate void ClientRecivedMessageNotify(string text);
    public delegate void IsClientStatusNotify(bool isConnected);

    public class ClientClass
    {
        private string _ip;
        private int _port;
        private string _username;
        private TcpClient _client;
        private bool _isWorking;

        private StreamReader? _reader = null;
        private StreamWriter? _writer = null;

        public event ClientRecivedMessageNotify? OnReceiveMessageEvent;
        public event IsClientStatusNotify? OnClientIsConnected;

        public ClientClass(string ip, string port, string username) 
        {
            _ip = ip;
            _port = Convert.ToInt32(port);   
            _username = username;
            _client = new TcpClient();
        }


        #region Подключаем клиента
        public async void ConnectAsync() 
        {
            try
            {
                _client.Connect(_ip, _port);
                if (_client.Connected)
                {
                    OnClientIsConnected?.Invoke(true);
                    _reader = new StreamReader(_client.GetStream());
                    _writer = new StreamWriter(_client.GetStream());
                    _isWorking = true;
                    await SendMessageAsync(_username);
                    await ReceiveMessageAsync();
                }
                else
                {
                    OnClientIsConnected?.Invoke(false);
                }
            }
            catch 
            {
                OnClientIsConnected?.Invoke(false);
                Disconnect();
            }

        }
        #endregion


        #region Примник сообщений
        async Task ReceiveMessageAsync()
        {
            while (_isWorking)
            {
                try
                {
                    string? message = await _reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(message)) continue;
                    else OnReceiveMessageEvent?.Invoke(message);
                }
                catch
                {
                    Disconnect();
                    break;
                }
            }
            
        }
        #endregion


        #region Отпраляем обычное сообщение на сервер
        public async Task SendMessageAsync(string message)
        {
            if(_writer != null)
            {
                await _writer.WriteLineAsync(message);
                await _writer.FlushAsync();
            }
        }
        #endregion

        #region Отключаем клиента 
        public void Disconnect()
        {
            _isWorking = false;
            _client.Close();
            _writer?.Close();
            _reader?.Close();
            if (!_client.Connected)
            {
                OnClientIsConnected?.Invoke(false);
            }
        }
        #endregion

    }
}
