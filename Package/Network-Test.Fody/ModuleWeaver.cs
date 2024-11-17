using System;
using System.Collections.Generic;
using Fody;


public class ModuleWeaver : BaseModuleWeaver
{
    public override void Execute()
    {
        System.Diagnostics.Debugger.Launch();
        var test = ModuleDefinition.Assembly.Name;
        Console.WriteLine("AHHHH");
        WriteError($"Assembly: {test}");
        throw new Exception("Shit");
    }

    public override IEnumerable<string> GetAssembliesForScanning()
    {
        yield return "netstandard";   
    }
}
