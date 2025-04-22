using System;

namespace AttributeNetworkWrapper.Core
{
    /// <summary>
    /// Rpc is reliable or Unreliable
    /// </summary>
    public enum SendType : ushort
    {
        Reliable,
        Unreliable
    }
    
    /// <summary>
    /// Rpc from Client to Server
    /// ServerRpc can have a ClientNetworkConnection as a parameter, where we pass the sender connection.
    /// When calling this, pass null for any ClientNetworkConnection parameter as they are not used.
    /// </summary>
    public class ServerRpc : Attribute
    {
        SendType sendType;

        public ServerRpc(SendType sendType = SendType.Reliable)
        {
            this.sendType = sendType;
        }
    }

    /// <summary>
    /// Rpc from Server to Client.
    /// First Param MUST be a ClientNetworkConnection, which will be the user this is sent to.
    /// </summary>
    public class ClientRpc : Attribute
    {
        SendType sendType;

        public ClientRpc(SendType sendType = SendType.Reliable)
        {
            this.sendType = sendType;
        }
    }
    
    
    /// <summary>
    /// Rpc from Server to all Clients
    /// </summary>
    public class MultiRpc : Attribute
    {
        SendType sendType;
        public MultiRpc(SendType sendType = SendType.Reliable)
        {
            this.sendType = sendType;
        }
    }
}