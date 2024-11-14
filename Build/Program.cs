
using Mono.Cecil;
using Test_Networking_Stuff.Attributes;
using Test_Networking_Stuff.Core.CodeGeneration;

namespace Build;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            throw new Exception("Insufficient arguments, pass in $(OutputPath) as an argument.");
        }
        
        string outputPath = args[0];
        
        if (!File.Exists(outputPath))
        {
            throw new Exception("Invalid Output Path, make sure the path is correct and this is a on output update event.");
        }
          
        MethodInjector.InitInstructionsList();
        ProcessAssembly(outputPath);
    }
    
    static void ProcessAssembly(string path)
    {
        AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(path);
        List<MethodDefinition> methods = new();
        
        foreach (var type in assembly.MainModule.Types)
        {
            foreach (var methodDefinition in type.Methods)
            {
                if(!methodDefinition.IsStatic) continue;
             
                if(methodDefinition.CustomAttributes.Any(attr => attr.AttributeType.IsType<ServerRpc>()))
                {
                   methods.Add(methodDefinition);
                }
            }
        }
        
        foreach (var methodDefinition in methods)
        {
            MethodInjector.InjectRpcCall(methodDefinition.DeclaringType, methodDefinition);
        }
        
        assembly.Write(path.Replace(".dll", "Built.dll"));
    }

}