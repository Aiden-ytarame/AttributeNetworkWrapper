using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AttributeNetworkWrapper.Core;

/// <summary>
/// Handles registering and calling received Rpc's.
/// </summary>
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
        //on build we generate this type, its static constructor calls RpcHandler.RegisterRpc() for all rpc's declared on it.
        //here we make sure their static constructor has ran
        Type registerRpcs = null;
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            registerRpcs = assembly.GetType("AttributeNetworkWrapper.RpcFuncRegistersGenerated");
            if (registerRpcs != null)
            {
                break;
            }
        }

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