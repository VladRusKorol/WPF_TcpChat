using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.Common
{
    public class ChatClient
    {
        private TcpClient? _tcpClient;

        private string? _name;

        public Guid Id { get; set; }
        protected internal StreamWriter? _writer { get; }
        protected internal StreamReader? _reader { get; }

        private bool _isFirstMessage;

        public event delMessageNotify OnMessageNotify;
        public event delCommandDeleteClientNotify OnDeleteClientNotify;

        public ChatClient(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
            _isFirstMessage = false;
            Id = Guid.NewGuid();
            var stream = tcpClient.GetStream();
            _writer = new StreamWriter(stream);
            _reader = new StreamReader(stream);
        }

        public async Task ProcessAsync()
        {
            try
            {
                while (_tcpClient.Connected)
                {
                    try
                    {
                        string? message = await _reader.ReadLineAsync();
                        if (message == null)
                        {
                            OnMessageNotify?.Invoke($"{_name} покинул в чат");
                            break;
                        }
                        if (!_isFirstMessage)
                        {
                            _isFirstMessage = true;
                            _name = message;
                            OnMessageNotify?.Invoke($"{_name} вошел в чат");
                        }
                        else
                        {
                            OnMessageNotify?.Invoke($"{_name} : {message}");
                        }
                    }
                    catch (Exception)
                    {
                        OnMessageNotify?.Invoke($"{_name} покинул в чат");
                        break;
                    }
                }
            }
            catch (Exception)
            { }
            finally 
            {
                OnDeleteClientNotify?.Invoke(Id);
            }
        }

        protected internal void Close()
        {
            _writer?.Close();
            _reader?.Close();
            _tcpClient?.Close();
        }
    }
}
