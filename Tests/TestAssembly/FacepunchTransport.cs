/*using Network_Test;
using Steamworks.Data;
using SendType = Network_Test.Core.Rpc.SendType;

namespace TestAssembly;

public class FacepunchTransport : Transport
{
    public override void ConnectClient(string address)
    {
        throw new NotImplementedException();
    }

    public override void StopClient()
    {
        throw new NotImplementedException();
    }


    public override void StartServer()
    {
        throw new NotImplementedException();
    }

    public override void StopServer()
    {
        throw new NotImplementedException();
    }

    public override void KickConnection(int connectionId)
    {
        throw new NotImplementedException();
    }

    public override void SendMessageToServer(ArraySegment<byte> data, SendType sendType = SendType.Reliable)
    {
        throw new NotImplementedException();
    }

    public override void SendMessageToClient(int connectionId, ArraySegment<byte> data, SendType sendType = SendType.Reliable)
    {
        throw new NotImplementedException();
    }

    public override void Shutdown()
    {
        throw new NotImplementedException();
    }
}

public class SocketManager : Steamworks.SocketManager
{
    public override void OnConnected(Connection connection, ConnectionInfo info)
    {
        base.OnConnected(connection, info);
    }

    public override void OnDisconnected(Connection connection, ConnectionInfo info)
    {
        base.OnDisconnected(connection, info);
    }

    public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime,
        int channel)
    {
        base.OnMessage(connection, identity, data, size, messageNum, recvTime, channel);
        //Transport.Instance.OnServerDataReceived?.Invoke(1, );
    }
}*/