using System;

namespace Network_Test.Core.Rpc
{
    public enum SendType : ushort
    {
        Reliable,
        Unreliable
    }
    
    public class ServerRpc : Attribute
    {
        SendType sendType;

        public ServerRpc(SendType sendType = SendType.Reliable)
        {
            this.sendType = sendType;
        }
    }
    public class ClientRpc : Attribute
    {
        SendType sendType;
        
        public ClientRpc(SendType sendType = SendType.Reliable)
        {
            this.sendType = sendType;
        }
    }
    public class MultiRpc : Attribute
    {
        SendType sendType;
        public MultiRpc(SendType sendType = SendType.Reliable)
        {
            this.sendType = sendType;
        }
    }
}