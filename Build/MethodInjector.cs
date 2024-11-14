using System.Diagnostics;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using ParameterAttributes = Mono.Cecil.ParameterAttributes;

namespace Build;


public static class MethodInjector
{
    private static Collection<Instruction> SendMessageInstructions;

    public static void InitInstructionsList()
    {
        TypeDefinition injectorType = AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)
            .MainModule.Types.First(att => att.Name == "MethodInjector");
        
        SendMessageInstructions = GetLocalMethod(injectorType, nameof(SendMessageToServerInstructions));
    }
    
    static  Collection<Instruction> GetLocalMethod(TypeDefinition type, string name)
    {
        return type.Methods.First(att => att.Name == name).Body.Instructions;
    }
    
    //taken from Mirror
    private static string GenerateMethodName(string initialPrefix, MethodDefinition md)
    {
        initialPrefix += md.Name;

        foreach (var parameter in md.Parameters)
        {
            initialPrefix += $"__{parameter.ParameterType.Name}";
        }

        return initialPrefix;
    }

    public static MethodDefinition InjectRpcCall(TypeDefinition typedDef, MethodDefinition methodDef)
    {
        string methodName = GenerateMethodName("OriginalFunc_", methodDef);
        MethodDefinition originalFunc = new MethodDefinition(methodName, methodDef.Attributes, methodDef.ReturnType);
      
        foreach (ParameterDefinition pd in methodDef.Parameters)
        {
            originalFunc.Parameters.Add(new ParameterDefinition(pd.Name, ParameterAttributes.None, pd.ParameterType));
        }
    
        (originalFunc.Body, methodDef.Body) = (methodDef.Body, originalFunc.Body);
        
        typedDef.Methods.Add(originalFunc);
    
        ILProcessor processor = methodDef.Body.GetILProcessor();
        foreach (Instruction instruction in SendMessageInstructions)
        {
            processor.Append(instruction);
        }
      
        return originalFunc;
    }

    private static void SendMessageToServerInstructions()
    {
        Console.WriteLine("Sending message to server");
        Console.ReadLine();
    }
}