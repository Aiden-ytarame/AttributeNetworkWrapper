using System;
using Network_Test.Core;
using Network_Test.Core.Rpc;

namespace Network_Test
{
    public abstract class Transport
    {
        public static Transport Instance { get; set; }
        
        public bool IsActive { get; internal set; }
        
        //CLIENT
        public Action<ArraySegment<byte>> OnClientDataReceived;
        public Action<ServerNetworkConnection> OnClientConnected;
        public Action OnClientDisconnected;
      
        public abstract void ConnectClient(string address);
        public abstract void StopClient();
        
     
        //SERVER
        public Action<ClientNetworkConnection, ArraySegment<byte>> OnServerDataReceived;
        public Action<ClientNetworkConnection> OnServerClientConnected;
        public Action<ClientNetworkConnection> OnServerClientDisconnected;
        public Action OnServerStarted;
        
        public abstract void StartServer();
        public abstract void StopServer();
        public abstract void KickConnection(int connectionId);
     
        
        public abstract void SendMessageToServer(ArraySegment<byte> data, SendType sendType = SendType.Reliable);
        public abstract void SendMessageToClient(int connectionId, ArraySegment<byte> data, SendType sendType = SendType.Reliable);
        public abstract void Shutdown();
    }
}