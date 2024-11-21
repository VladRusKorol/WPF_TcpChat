using Client.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Client
{
    public class MainWindowViewModel: BaseINPC
    {
        private ClientClass? _client;

        private string? _port;
        public string? Port { get => this._port; set => SetField(ref _port, value); }

        private string? _ip; 
        public string? Ip { get => this._ip; set => SetField(ref _ip, value); }

        private string? _userName;
        public string? UserName { get => this._userName; set => SetField(ref _userName, value); }

        private string? _message;
        public string? Message { get => this._message; set => SetField(ref _message, value); }

        private ObservableCollection<string> _chatElements = new ObservableCollection<string>();

        public ObservableCollection<string> ChatElements
        {
            get => this._chatElements;
            set => SetField(ref _chatElements, value);
        }

        private ChatElement _selectedChatElement;
        public ChatElement SelectedChatElement
        {
            get => this._selectedChatElement;
            set => SetField(ref _selectedChatElement, value);
        }

        private ClientStatus _status;
        public ClientStatus Status
        {
            get => this._status;
            set
            {
                switch (value) 
                { 
                    case ClientStatus.Connected:
                        {
                            StatusLabel = value.ToString();
                            StatusLabelColor = Brushes.Green;
                            break;
                        }
                    case ClientStatus.Disconected: 
                        {
                            StatusLabel = value.ToString();
                            StatusLabelColor = Brushes.Gray;
                            break;
                        }
                    case ClientStatus.Error:
                        {
                            StatusLabel = value.ToString();
                            StatusLabelColor = Brushes.Red;
                            break;
                        }
                    default:
                        {
                            StatusLabel = value.ToString();
                            StatusLabelColor = Brushes.Gray;
                            break;
                        }
                }
                SetField(ref _status, value);
            }
        }


        private string? _statusLabel;
        public string? StatusLabel
        {
            get => this._statusLabel;
            set => SetField(ref _statusLabel, value);
        }

        private string? _text;
        public string? Text
        {
            get => this._text;
            set => SetField(ref _text, value);
        }

        private string? _name;
        public string? Name
        {
            get => this._name;
            set => SetField(ref _name, value);
        }


        private SolidColorBrush? _statusLabelColor;
        public SolidColorBrush? StatusLabelColor
        {
            get => this._statusLabelColor;
            set => SetField(ref _statusLabelColor, value);
        }


        public LambdaCommand CommandConnect { get; set; }
        public LambdaCommand CommandDisconnect { get; set; }
        public LambdaCommand CommandClear { get; set; }
        public LambdaCommand CommandSendMessage { get; set; }


        public MainWindowViewModel()
        {
            Status = ClientStatus.Initialize;

            Ip = IPAddress.Loopback.ToString();
            Port = "0";

            CommandConnect = new LambdaCommand(

                canExecute: _ =>
                                !string.IsNullOrEmpty(Ip) &&
                                !string.IsNullOrEmpty(Port) &&
                                !string.IsNullOrEmpty(UserName) &&
                                (Status == ClientStatus.Initialize || Status == ClientStatus.Disconected || Status == ClientStatus.Error),
                execute: _ =>
                {
                    if (checkedIp(Ip) && checkedPort(Port))
                    {
                        _client = new ClientClass(Ip, Port, UserName);
                        _client.OnReceiveMessageEvent += OnRecivedMessage;
                        _client.OnClientIsConnected += OnClientIsConnected;
                        _client.ConnectAsync();
                    }
                    else
                    {
                        Status = ClientStatus.Error;
                    }
                }
            );

            CommandDisconnect = new LambdaCommand(

                canExecute: _ => Status == ClientStatus.Connected,
                execute: _ =>
                {
                    _client.Disconnect();
                    _client = null;
                }
            );

            CommandClear = new LambdaCommand(
                canExecute: _ => (Status == ClientStatus.Initialize || Status == ClientStatus.Disconected || Status == ClientStatus.Error),
                execute: _ =>
                {
                    Ip = IPAddress.Loopback.ToString();
                    Port = "0";
                    UserName = "";
                }

            );

            CommandSendMessage = new LambdaCommand(
                 canExecute: _ => Status == ClientStatus.Connected && _client is not null,
                 execute: async _ =>
                 {
                     if (_client is not null)
                     {
                         await _client.SendMessageAsync(Message);
                     }
                 }

             );

        }

        #region Проверка порта и Ip адреса
        private bool checkedIp(string ip) 
        {
            Match match = Regex.Match(ip, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}");
            if (match.Success) 
            { 
                return true;
            }
            return false;
        }


        private bool checkedPort(string port)
        {
            bool res1 = int.TryParse(Port, out int port2);
            if (res1)
            {
                return port2 > 0 && port2 < 65535;
            }
            return false;
        }
        #endregion

        #region Подписка на получения статуса подключения клиента
        private void OnClientIsConnected(bool isConnected)
        {
            if (isConnected) Status = ClientStatus.Connected;
            else Status = ClientStatus.Disconected;
        }
        #endregion

        #region Подписка на получения сообщение с сервера
        private void OnRecivedMessage(string message)
        {
            ChatElements.Add(message);
        }
        #endregion

    }
}
