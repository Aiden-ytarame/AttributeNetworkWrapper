using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;
using Network_Test.Fody.Core;
using TypeSystem = Mono.Cecil.TypeSystem;


public class ModuleWeaver : BaseModuleWeaver
{
    public static TypeDefinition NetworkManagerType;
    public static TypeDefinition NetworkWriterType;
    public static TypeDefinition NetworkReaderType;
    public static TypeDefinition NetworkWriterExtensionType;
    public static TypeDefinition NetworkReaderExtensionType;
    
    public static TypeDefinition ServerNetworkConnectionType;
    public static TypeDefinition ClientNetworkConnectionType;
   
    public static TypeDefinition ServerRpcAttr;
    public static TypeDefinition ClientRpcAttr;
    public static TypeDefinition MultiRpcAttr;

    public static TypeDefinition RpcHandlerType;
    public static TypeDefinition RpcDelegateType;

    private static ModuleWeaver Inst;
    public static TypeSystem TypeSystem => Inst.ModuleDefinition.TypeSystem;
    public static ModuleDefinition Module => Inst.ModuleDefinition;
    public override void Execute()
    {
        FindTypeFromMainAssembly("Network_Test.NetworkManager", out NetworkManagerType);
        FindTypeFromMainAssembly("Network_Test.Core.NetworkWriter", out NetworkWriterType);
        FindTypeFromMainAssembly("Network_Test.Core.NetworkReader", out NetworkReaderType);
        FindTypeFromMainAssembly("Network_Test.Core.NetWriterExtensions", out NetworkWriterExtensionType);
        FindTypeFromMainAssembly("Network_Test.Core.NetReaderExtensions", out NetworkReaderExtensionType);
        FindTypeFromMainAssembly("Network_Test.Core.Rpc.ServerRpc", out ServerRpcAttr);
        FindTypeFromMainAssembly("Network_Test.Core.Rpc.ClientRpc", out ClientRpcAttr);
        FindTypeFromMainAssembly("Network_Test.Core.Rpc.MultiRpc", out MultiRpcAttr);
        FindTypeFromMainAssembly("Network_Test.NetworkManager", out NetworkManagerType);
        FindTypeFromMainAssembly("Network_Test.Core.RpcHandler", out RpcHandlerType);
        FindTypeFromMainAssembly("Network_Test.Core.ServerNetworkConnection", out ServerNetworkConnectionType);
        FindTypeFromMainAssembly("Network_Test.Core.ClientNetworkConnection", out ClientNetworkConnectionType);
        //FindTypeFromMainAssembly("Network_Test.Core.RpcHandler/RpcDelegate", out RpcDelegateType);

        RpcDelegateType = RpcHandlerType.NestedTypes.First(x => x.Name == "RpcDelegate");
        
        Inst = this;
        
        ReadersWriters.PopulateWriterMethods(ModuleDefinition);
        ReadersWriters.PopulateReadersMethods(ModuleDefinition);
        
        Processor.Initialize(ModuleDefinition);
        
        foreach (var type in ModuleDefinition.Types)
        {
            ProcessType(type);
        }
        
        Processor.WriteRpcsToConstructor();
        
        WriteInfo("Weaving Done.");
    }

    void FindTypeFromMainAssembly(string typeFullName, out TypeDefinition type)
    {
        if (!TryFindTypeDefinition(typeFullName, out type))
            throw new WeavingException($"Failed to find type [{typeFullName}] from main assembly.");

    }
    void ProcessType(TypeDefinition type)
    {
        for (int i = 0; i < type.Methods.Count; i++)
        {
            ProcessMethod(type.Methods[i]);
        }
    }

    void ProcessMethod(MethodDefinition method)
    {
        Processor.ProcessMethod(method);
    }
    
    
    public override IEnumerable<string> GetAssembliesForScanning()
    {
        yield return "netstandard";
        yield return "Network-Test";
    }

    public static TypeReference Import<T>()
    {
        return Inst.ModuleDefinition.ImportReference(typeof(T));
    }
    
    public override bool ShouldCleanReference => false;
}