using System.Collections.Generic;
using System.Linq;
using AttributeNetworkWrapper.Fody.Core;
using Fody;
using Mono.Cecil;
using TypeSystem = Mono.Cecil.TypeSystem;


namespace AttributeNetworkWrapper.Fody
{
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
            FindTypeFromMainAssembly("AttributeNetworkWrapper.NetworkManager", out NetworkManagerType);
            FindTypeFromMainAssembly("AttributeNetworkWrapper.Core.NetworkWriter", out NetworkWriterType);
            FindTypeFromMainAssembly("AttributeNetworkWrapper.Core.NetworkReader", out NetworkReaderType);
            FindTypeFromMainAssembly("AttributeNetworkWrapper.Core.NetWriterExtensions", out NetworkWriterExtensionType);
            FindTypeFromMainAssembly("AttributeNetworkWrapper.Core.NetReaderExtensions", out NetworkReaderExtensionType);
            FindTypeFromMainAssembly("AttributeNetworkWrapper.Core.ServerRpc", out ServerRpcAttr);
            FindTypeFromMainAssembly("AttributeNetworkWrapper.Core.ClientRpc", out ClientRpcAttr);
            FindTypeFromMainAssembly("AttributeNetworkWrapper.Core.MultiRpc", out MultiRpcAttr);
            FindTypeFromMainAssembly("AttributeNetworkWrapper.NetworkManager", out NetworkManagerType);
            FindTypeFromMainAssembly("AttributeNetworkWrapper.Core.RpcHandler", out RpcHandlerType);
            FindTypeFromMainAssembly("AttributeNetworkWrapper.Core.ServerNetworkConnection", out ServerNetworkConnectionType);
            FindTypeFromMainAssembly("AttributeNetworkWrapper.Core.ClientNetworkConnection", out ClientNetworkConnectionType);
            //FindTypeFromMainAssembly("AttributeNetworkWrapper.Core.RpcHandler/RpcDelegate", out RpcDelegateType);

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
            yield return "AttributeNetworkWrapper";
        }

        public static TypeReference Import<T>()
        {
            return Inst.ModuleDefinition.ImportReference(typeof(T));
        }
    
        public override bool ShouldCleanReference => false;
    }
}