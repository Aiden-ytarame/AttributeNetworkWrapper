using System;
using AttributeNetworkWrapper.Core;

namespace Tests;

public class testTransport : Transport
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
        Console.Write("LESGOO");
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