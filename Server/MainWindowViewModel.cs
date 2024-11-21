using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Server.Common;
using System.Collections.ObjectModel;

namespace Server
{
    public class MainWindowViewModel: BaseINPC
    {
        //VIEW MODEL VARIABLES

        private ChatServer? _serverObj;


        private string? _port;
        public string? Port
        {
            get => this._port;
            set => SetField(ref _port, value);
        }

        private string? _ipAddress;
        public string? IpAddress {
            get => this._ipAddress;
            set => SetField(ref _ipAddress, value);
        }

        private ServerStatus _status = ServerStatus.Disconnected;
        public ServerStatus Status
        {
            get => this._status;
            set 
            {
                SetField(ref _status, value);
            } 
        }

        private string _statusMSG;
        public string StatusMSG
        {
            get => this._statusMSG;
            set => SetField(ref _statusMSG, value);
        }

        private SolidColorBrush _statusForegroudColor;
        public SolidColorBrush StatusForegroudColor
        {
            get => this._statusForegroudColor;
            set => SetField(ref _statusForegroudColor, value);
        }


        private int _currentClientOnline;
        public int CurrentClientOnline
        {
            get => this._currentClientOnline;
            set => SetField(ref _currentClientOnline, value);
        }



        private int _countConnectedUsers;
        public int CountConnectedUsers
        {
            get => this._countConnectedUsers;
            set => SetField(ref _countConnectedUsers, value);
        }

        private int _countDisconnectedUsers;
        public int CountDisconnectedUsers
        {
            get => this._countDisconnectedUsers;
            set => SetField(ref _countDisconnectedUsers, value);
        }

        private int _countsSendMessages;
        public int CountsSendMessages
        {
            get => this._countsSendMessages;
            set => SetField(ref _countsSendMessages, value);
        }

        public LambdaCommand CommandCheckAndStart { get; set; }
        public LambdaCommand CommandStopServer { get; set; }


        public MainWindowViewModel()
        {

            IpAddress = IPAddress.Loopback.ToString();
            Status = ServerStatus.Disconnected;

             CommandCheckAndStart = new LambdaCommand(

                canExecute: _ => !String.IsNullOrEmpty(Port) && !String.IsNullOrEmpty(IpAddress) && (Status == ServerStatus.Disconnected || Status == ServerStatus.Error),
                execute: async _ =>
                {
                    bool flagPort = true, flagAddress = true;

                    if (!int.TryParse(Port, out int port))
                    {
                        flagPort = false;
                    }
                    else if (port < 0 || port > 65535)
                    {
                        flagPort = false;
                    }
                    else 
                    {
                        flagPort = true;
                    }

                    Match match = Regex.Match(IpAddress, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}");
                    if (match.Success)
                    {
                        flagAddress = true;
                    }
                    else
                    {
                        flagAddress = false;
                    }

                    if(flagPort == true && flagAddress == true )
                    {
                        Status = ServerStatus.Connected;
                        StatusForegroudColor = Brushes.Green;
                        StatusMSG = "";


                        IPAddress ip = IPAddress.Parse(IpAddress);
                        _serverObj = new ChatServer(ip, port);
                        _serverObj.CountClientConnect += CountConnectedClient;
                        _serverObj.CountClientDisconnect += CountDisconnectedClient;
                        _serverObj.CountSendMessage += CountSendMessage;
                        _serverObj.CountClientOnline += CountClientOnline;
                        await _serverObj.ServerStartAndListenAsync();
                    }
                    else
                    {
                        if (flagPort == false && flagAddress == true) StatusMSG = "Invalid port";
                        else if (flagPort == true && flagAddress == false) StatusMSG = "Invalid address";
                        else if (flagPort == false && flagAddress == false) StatusMSG = "Invalid port and address";
                        Status = ServerStatus.Error;
                        StatusForegroudColor = Brushes.Red;
                    }
                }

            );

            CommandStopServer = new LambdaCommand(
                canExecute: _ => Status == ServerStatus.Connected,
                execute: _ =>
                {
                    Status = ServerStatus.Disconnected;
                    StatusForegroudColor = Brushes.Gray;
                    CountConnectedUsers = 0;
                    CountDisconnectedUsers = 0;
                    CurrentClientOnline = 0;
                    CountsSendMessages = 0;
                    _serverObj.ServerStop();
                }
            );



        }


        private void CountConnectedClient(int count)
        {
            CountConnectedUsers = count;
        }

        private void CountDisconnectedClient(int count)
        {
            CountDisconnectedUsers = count;
        }

        private void CountSendMessage(int count)
        {
            CountsSendMessages = count;
        }


        private void CountClientOnline(int count)
        {
            CurrentClientOnline = count;
        }

    }
}
