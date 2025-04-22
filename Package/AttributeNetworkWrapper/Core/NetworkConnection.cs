using System;

namespace AttributeNetworkWrapper.Core
{
    /// <summary>
    /// Class representing a connection.
    /// Contains abstract Send and Disconnect functions.
    /// </summary>
    public abstract class NetworkConnection
    {
        public readonly int ConnectionId = 0;
        public string Address { get; private set; }
        
        internal NetworkConnection(){}
        internal NetworkConnection(int connectionId, string address)
        {
            ConnectionId = connectionId;
            Address = address;
        }
       
        public abstract void SendRpcToTransport(ArraySegment<byte> data, SendType sendType = SendType.Reliable);
        public abstract void Disconnect();
    }
    /// <summary>
    /// Class representing a connection to a Server
    /// </summary>
    /// <param name="address">Adress to the server</param>
    public class ServerNetworkConnection(string address) : NetworkConnection(0, address)
    {
        public override void SendRpcToTransport(ArraySegment<byte> data, SendType sendType = SendType.Reliable) => Transport.Instance.SendMessageToServer(data, sendType);
        public override void Disconnect()
        {
            NetworkManager.Instance.Disconnect();
        }
    }
    
    /// <summary>
    /// Class representing a connection to a Client
    /// </summary>
    /// <param name="connectionId">Id of this client</param>
    /// <param name="address">Address of this client</param>
    public class ClientNetworkConnection(int connectionId, string address) : NetworkConnection(connectionId, address)
    {
        public override void SendRpcToTransport(ArraySegment<byte> data, SendType sendType = SendType.Reliable) => Transport.Instance.SendMessageToClient(ConnectionId, data, sendType);

        public override void Disconnect()
        {
            NetworkManager.Instance.KickClient(ConnectionId);
        }
    }
    
}