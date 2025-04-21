using Network_Test.Core.Rpc;

namespace Network_Test.Core
{
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
    public class ServerNetworkConnection : NetworkConnection
    {
        public override void SendRpcToTransport(ArraySegment<byte> data, SendType sendType = SendType.Reliable) => Transport.Instance.SendMessageToServer(data, sendType);
        public override void Disconnect()
        {
            throw new System.NotImplementedException();
        }
    }
    public class ClientNetworkConnection : NetworkConnection
    {
        public ClientNetworkConnection(int connectionId, string address) : base(connectionId, address)
        {
        }
        public override void SendRpcToTransport(ArraySegment<byte> data, SendType sendType = SendType.Reliable) => Transport.Instance.SendMessageToClient(ConnectionId, data, sendType);

        public override void Disconnect()
        {
            throw new System.NotImplementedException();
        }
    }
    
}