using System;
using Network_Test_Common.Core;
using Network_Test.Core;
using Network_Test.Core.Rpc;

namespace Network_Test
{
    public class NetworkManager
    {
        public static NetworkManager Instance { get; private set; }
        
        ServerNetworkConnection? _serverConnection;
        Dictionary<int, ClientNetworkConnection> _clientConnections = new();

        bool _eventsSet = false;
        private Transport Transport => Transport.Instance;

        bool Init(Transport transport)
        {
            if (Instance != null)
            {
                return false;
            }
            Instance = this;
            Transport.Instance = transport;
            SetupEvents();
            
            return true;
        }

        void SetupEvents()
        {
            Transport.OnClientConnected += OnClientConnected;
            Transport.OnClientDisconnected += OnClientDisconnected;
            Transport.OnServerClientConnected += OnServerClientConnected;
            Transport.OnServerClientDisconnected += OnServerClientDisconnected;
            Transport.OnServerStarted += OnServerStarted;
            _eventsSet = true;
        }

        void DeSetupEvents()
        {
            if (!_eventsSet)
            {
                return;
            }
            
            Transport.OnClientConnected -= OnClientConnected;
            Transport.OnClientDisconnected -= OnClientDisconnected;
            Transport.OnServerClientConnected -= OnServerClientConnected;
            Transport.OnServerClientDisconnected -= OnServerClientDisconnected;
            Transport.OnServerStarted -= OnServerStarted;
            _eventsSet = false;
        }
        
        //client
        public virtual void ConnectToServer(string address)
        {
            Transport.ConnectClient(address);
        }

        public virtual void Disconnect()
        {
            Transport.StopClient();
        }

        public virtual void OnClientConnected() {}
        public virtual void OnClientDisconnected() {}
        
        public virtual void SendToServer(ArraySegment<byte> data, SendType sendType)
        {
            if (_serverConnection == null)
            {
                throw new NullServerException("Tried calling a server rpc while server is Null!");
            }
            
            _serverConnection.SendRpcToTransport(data, sendType);
        }
        
        //server
        public virtual void OnServerClientDisconnected(int connectionId, string address)
        {
            _clientConnections.Remove(connectionId);
        }

        public virtual void OnServerClientConnected(int connectionId, string address)
        {
            _clientConnections.Add(connectionId, new ClientNetworkConnection(connectionId, address));
        }
        public virtual void OnServerStarted() {}
        
        public virtual void SendToClient(int connectionId, ArraySegment<byte> data, SendType sendType)
        {
            if (_clientConnections.TryGetValue(connectionId, out var client))
            {
                client.SendRpcToTransport(data, sendType);
                return;
            }

            throw new ArgumentException("Tried to send rpc to invalid connection ID!");
        }
        
    }
}