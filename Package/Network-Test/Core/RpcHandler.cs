using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Network_Test.Core;

public static class RpcHandler
{
    public enum CallType
    {
        Server,
        Client,
        Multi
    }
    
    public struct Invoker(CallType callType, RpcDelegate func)
    {
        public CallType CallType = callType;
        public RpcDelegate RpcFunc = func;
    }
    
    
    public delegate void RpcDelegate(ClientNetworkConnection conn, NetworkReader reader);
    static Dictionary<ushort, Invoker> RpcInvokers = new();

    static RpcHandler()
    {
        var registerRpcs = Type.GetType("Network_Test.RpcFuncRegistersGenerated");
        if (registerRpcs == null)
        {
            throw new ApplicationException("RpcFuncRegistersGenerated wasn't found, something terrible went wrong");
        }
        
        RuntimeHelpers.RunClassConstructor(registerRpcs.TypeHandle);
    }
    
    public static bool TryGetRpcInvoker(ushort hash, out Invoker invoker)
    {
        return RpcInvokers.TryGetValue(hash, out invoker);
    }
    
    public static void RegisterRpc(ushort hash, RpcDelegate rpcDelegate, CallType callType)
    {
        RpcInvokers.Add(hash, new Invoker(callType, rpcDelegate));
    }
}