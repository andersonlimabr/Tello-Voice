using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TelloVoice
{
    public class TelloProxy
    {
        private UdpClient _client;
        private IPAddress _ipaddress;
        private IPEndPoint _endpoint;
        private IPEndPoint _remoteIpEndPoint;
        private string _serverReponse;
        private bool _commandMode = false;

        public string Host => _ipaddress.ToString();
        public TelloProxy(string ipAddress, int port)
        {
            _client = new UdpClient();
            _ipaddress = IPAddress.Parse(ipAddress);
            _endpoint = new IPEndPoint(_ipaddress,port);
            _remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        }

        public string ServerResponse => _serverReponse;
        public bool CommandModeEnabled => _commandMode;

        public bool Connect()
        {
            _serverReponse= ExecuteCommand("command");

            if (_serverReponse == "FAIL")
            {
                _commandMode = false;
            }
            else
            {
                _commandMode = true;
            }

            return _commandMode;
        }

        public bool Decolar()
        {
            _serverReponse = ExecuteCommand("takeoff");

            if (_serverReponse == "FAIL")
            {
                _commandMode = false;
            }
            else
            {
                _commandMode = true;
            }

            return _commandMode;
        }

        public bool Pousar()
        {
            _serverReponse = ExecuteCommand("land");

            if (_serverReponse == "FAIL")
            {
                _commandMode = false;
            }
            else
            {
                _commandMode = true;
            }

            return _commandMode;
        }

        private string ExecuteCommand(string command)
        {
            _client.Connect(_endpoint);
            var data = Encoding.ASCII.GetBytes(command);
            _client.Send(data, data.Length);
            _client.Client.ReceiveTimeout = 2500;
            var receiveBytes = _client.Receive(ref _remoteIpEndPoint);

            return Encoding.ASCII.GetString(receiveBytes);
        }

        public int GetBattery()
        {
            _serverReponse = ExecuteCommand("battery?");

            if (_serverReponse == "FAIL")
            {
                return 0;
            }
            else
            {
                return int.Parse(_serverReponse);
            }
        }
  
        public void Close()
        {
            _client.Close();
            _client.Dispose();
        }


    }
}
