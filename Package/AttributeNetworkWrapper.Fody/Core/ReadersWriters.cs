using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Fody;
using Mono.Cecil;

namespace AttributeNetworkWrapper.Fody.Core
{
    public static class ReadersWriters
    {
        private static readonly Dictionary<string, MethodReference> WriterFunctions =
            new Dictionary<string, MethodReference>();
        
        private static readonly Dictionary<string, MethodReference> ReaderFunctions =
            new Dictionary<string, MethodReference>();

        public static MethodReference GetWriterMethod(TypeReference type)
        {
            if (WriterFunctions.TryGetValue(type.FullName, out MethodReference writerMethod))
            {
                return writerMethod;
            }

            throw new WeavingException($"Type [{type.FullName}] doesn't have a writer method, consider making an extension method for NetworkWriter that takes [{type.Name}]");
        }
        
        public static MethodReference GetReaderMethod(TypeReference type)
        {
            if (ReaderFunctions.TryGetValue(type.FullName, out MethodReference writerMethod))
            {
                return writerMethod;
            }

            throw new WeavingException($"Type [{type.FullName}] doesn't have a reader method, consider making an extension method for NetworkReader that returns [{type.Name}]");
        }

        public static void PopulateWriterMethods(ModuleDefinition module)
        {
            //parse the Write methods in NetWriterExtensions
            TypeDefinition networkWriter = module.ImportReference(ModuleWeaver.NetworkWriterExtensionType).Resolve();
            foreach (var methodDefinition in networkWriter.Methods)
            {
                ParseWriterExtensionMethod(methodDefinition, module);
            }
           
            //parse any extension methods for NetworkWrite
            foreach (var type in module.Types)
            {
                foreach (var methodDefinition in type.Methods)
                {
                    ParseWriterExtensionMethod(methodDefinition, module);
                }
            }
        }

        static void ParseWriterExtensionMethod(MethodDefinition method, ModuleDefinition module)
        {
            if (!method.IsStatic || 
                !method.IsPublic ||
                !method.ReturnType.EqualsTo(typeof(void)) ||
                method.Parameters.Count != 2 ||
                !method.Parameters[0].ParameterType.EqualsTo(ModuleWeaver.NetworkWriterType) ||
                !method.CustomAttributes.HasAttribute<ExtensionAttribute>()
                )
            {
                return;
            }
            
            AddWriterMethod(method.Parameters[1].ParameterType, method, module);
        }
        public static void AddWriterMethod(TypeReference type, MethodReference writerMethod, ModuleDefinition module)
        {
            if (WriterFunctions.ContainsKey(type.FullName))
            {
                throw new WeavingException($"Type [{type.FullName}] already has a writer method");
            }
            
            WriterFunctions[type.FullName] = module.ImportReference(writerMethod);
        }

        public static void PopulateReadersMethods(ModuleDefinition module)
        {
            //parse the Write methods in NetWriterExtensions
            TypeDefinition networkWriter = module.ImportReference(ModuleWeaver.NetworkReaderExtensionType).Resolve();
            foreach (var methodDefinition in networkWriter.Methods)
            {
                ParseReaderExtensionMethod(methodDefinition, module);
            }
           
            //parse any extension methods for NetworkWrite
            foreach (var type in module.Types)
            {
                foreach (var methodDefinition in type.Methods)
                {
                    ParseReaderExtensionMethod(methodDefinition, module);
                }
            }
        }
        
        static void ParseReaderExtensionMethod(MethodDefinition method, ModuleDefinition module)
        {
            if (!method.IsStatic || 
                !method.IsPublic ||
                method.ReturnType.EqualsTo(typeof(void)) ||
                method.Parameters.Count == 0 ||
                !method.Parameters[0].ParameterType.EqualsTo(ModuleWeaver.NetworkReaderType) ||
                !method.CustomAttributes.HasAttribute<ExtensionAttribute>()
               )
            {
                return;
            }
            
            AddReaderMethod(method.ReturnType, method, module);
        }
        
        public static void AddReaderMethod(TypeReference type, MethodReference writerMethod, ModuleDefinition module)
        {
            if (ReaderFunctions.ContainsKey(type.FullName))
            {
                throw new WeavingException($"Type [{type.FullName}] already has a reader method");
            }
            
            ReaderFunctions[type.FullName] = module.ImportReference(writerMethod);
        }
    }
}