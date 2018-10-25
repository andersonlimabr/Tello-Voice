using System;
using System.Linq;
using System.Net;
using TelloSdkCoreNet.actions;

namespace TelloSdkCoreNet
{
    public class SdkWrapper
    {
        private TelloUdpClient _udpClient;
        private IPAddress _ipAddress;
        private IPEndPoint _endpoint;
        private Exception _lastException;
        private static readonly SdkWrapper _instance = new SdkWrapper();
        public enum SdkReponses
        {
            OK,
            FAIL
        }
        
        public FlipActions FlipActions;
        public FlyActions FlyActions;
        public RotationActions RotationActions;
        public BaseActions BaseActions;
        public SpeedAction SpeedAction;

        public Exception LastException => _lastException;

        public string ConnectedTo
        {
            get
            {
                return _udpClient == null ? "Disconnected"
                                          : $"Connected to:{_udpClient.Host}";
            }
        }

        public static SdkWrapper Instance => _instance;
        private SdkWrapper()
        {
            CreateClient();
            FlipActions = new FlipActions(_udpClient);
            FlyActions = new FlyActions(_udpClient);
            RotationActions = new RotationActions(_udpClient);
            BaseActions = new BaseActions(_udpClient);
            SpeedAction = new SpeedAction(_udpClient);
        }

        public void Shutdown()
        {
            _udpClient.Close();
        }

      
        public void CreateClient()
        {
            //_ipAddress = IPAddress.Parse("10.30.54.81");
            _ipAddress = IPAddress.Parse("192.168.10.1");
            _endpoint = new IPEndPoint(_ipAddress, 8889);
            _udpClient = new TelloUdpClient(_ipAddress,_endpoint);
        }
        
        
    }
}
