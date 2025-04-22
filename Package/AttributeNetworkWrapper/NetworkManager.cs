using System;
using System.Collections.Generic;
using AttributeNetworkWrapper.Core;

namespace AttributeNetworkWrapper
{
    /// <summary>
    /// Singleton responsible for forwarding rpc calls to the Transport and Handling received messages
    /// Theres many virtual functions for methods like OnClientConnected, just make sure to call base.function() on any overrides.
    /// </summary>
    public class NetworkManager
    {
        /// <summary>
        /// Singleton of the current NetworkManager
        /// </summary>
        public static NetworkManager Instance { get; private set; }
        /// <summary>
        /// Simply calls Transport.IsActive
        /// </summary>
        public bool TransportActive => Transport.IsActive;
        
        protected ServerNetworkConnection? _serverConnection;
        protected Dictionary<int, ClientNetworkConnection> _clientConnections = new();
        
        
        /// <summary>
        /// Calls Transport.Instance
        /// </summary>
        protected Transport Transport => Transport.Instance;
        
        bool _eventsSet = false;
        
        ~NetworkManager()
        {
            DeSetupEvents();
        }
        
        /// <summary>
        /// Inits this networkManager, must be called before use.
        /// Fails if an Instance already exists.
        /// </summary>
        /// <param name="transport">Transport instance to use</param>
        /// <returns>successfully init</returns>
        public bool Init(Transport transport)
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
            if (_eventsSet)
            {
                return;
            }
            
            Transport.OnClientConnected += OnClientConnected;
            Transport.OnServerStarted += OnServerStarted;
            
            Transport.OnClientDisconnected += OnClientDisconnected;
            Transport.OnServerClientConnected += OnServerClientConnected;
            Transport.OnServerClientDisconnected += OnServerClientDisconnected;
            
            Transport.OnServerDataReceived += OnServerTransportDataReceived;
            Transport.OnClientDataReceived += OnClientTransportDataReceived;
            _eventsSet = true;
        }

        void DeSetupEvents()
        {
            if (!_eventsSet)
            {
                return;
            }
            
            Transport.OnClientConnected -= OnClientConnected;
            Transport.OnServerStarted -= OnServerStarted;
            
            Transport.OnClientDisconnected -= OnClientDisconnected;
            Transport.OnServerClientConnected -= OnServerClientConnected;
            Transport.OnServerClientDisconnected -= OnServerClientDisconnected;
            
            Transport.OnServerDataReceived -= OnServerTransportDataReceived;
            Transport.OnClientDataReceived -= OnClientTransportDataReceived;
            _eventsSet = false;
        }
        
        // CLIENT ////////////////////////////////
        public virtual void ConnectToServer(string address)
        {
            Transport.ConnectClient(address);
        }

        public virtual void Disconnect()
        {
            Transport.StopClient();
        }

        public virtual void KickClient(int clientId)
        {
            Transport.KickConnection(clientId);
        }

        public virtual void OnClientConnected(ServerNetworkConnection connection)
        {
            _serverConnection = connection;
        }

        public virtual void OnClientDisconnected()
        {
            _serverConnection = null;
        }
        
        public virtual void SendToServer(ArraySegment<byte> data, SendType sendType)
        {
            if (_serverConnection == null || Transport == null || !Transport.IsActive)
            {
                throw new NullServerException("Tried calling a server rpc while server is Null!");
            }
            
            _serverConnection.SendRpcToTransport(data, sendType);
        }

        internal static void OnClientTransportDataReceived(ArraySegment<byte> data)
        {
            using NetworkReader reader = new NetworkReader(data);
            
            if (data.Count < 2)
            {
                Console.WriteLine("Data was too small, idk what we do for now");
                return;
            }
            ushort hash = reader.ReadUInt16();
            if (RpcHandler.TryGetRpcInvoker(hash, out var rpcHandler))
            {
                if (rpcHandler.CallType != RpcHandler.CallType.Server)
                {
                    rpcHandler.RpcFunc.Invoke(null, reader);
                }
                else
                {
                    Console.WriteLine("Received server rcp as a client!");
                }
            }
            else
            {
                Console.WriteLine("Received invalid id!");
            }
        }
        //server
        public virtual void StartServer()
        {
            Transport.StartServer();
        }

        public virtual void EndServer()
        {
            Transport.StopServer();
        }
        
        public virtual void OnServerClientDisconnected(ClientNetworkConnection connection)
        {
            _clientConnections.Remove(connection.ConnectionId);
        }

        public virtual void OnServerClientConnected(ClientNetworkConnection connection)
        {
            _clientConnections.Add(connection.ConnectionId, connection);
        }
        public virtual void OnServerStarted() {}
        
        public virtual void SendToClient(ClientNetworkConnection connection, ArraySegment<byte> data, SendType sendType)
        {
            if (Transport == null || !Transport.IsActive)
            {
                throw new NullServerException("Tried calling a Client rpc while transport is Null!");
            }

            if (connection != null)
            {
                connection.SendRpcToTransport(data, sendType);
                return;
            }
            
            throw new ArgumentException("Tried to send rpc to invalid connection ID!");
        }
        
        public virtual void SendToAllClients(ArraySegment<byte> data, SendType sendType)
        {
            if (Transport == null || !Transport.IsActive)
            {
                throw new NullServerException("Tried calling a Client rpc while transport is Null!");
            }
            
            foreach (var clientNetworkConnection in _clientConnections)
            {
                clientNetworkConnection.Value.SendRpcToTransport(data, sendType);
            }
        }
        
        internal static void OnServerTransportDataReceived(ClientNetworkConnection connection, ArraySegment<byte> data)
        {
            using NetworkReader reader = new NetworkReader(data);
            
            if (data.Count < 2)
            {
                Console.WriteLine("Data was too small, idk what we do for now");
                return;
            }
            ushort hash = reader.ReadUInt16();
            
            if (RpcHandler.TryGetRpcInvoker(hash, out var rpcHandler))
            {
                if (rpcHandler.CallType == RpcHandler.CallType.Server)
                {
                    rpcHandler.RpcFunc.Invoke(connection, reader);
                }
                else
                {
                    Console.WriteLine("Received client rcp as a server!");
                }
            }
            else
            {
                Console.WriteLine("Received invalid id!");
            }
        }
        
    }
}