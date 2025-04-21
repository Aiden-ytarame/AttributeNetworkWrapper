using System;
using Fody;
using Network_Test;
using Network_Test.Core;
using Tests;
using Xunit;

public class WeaverTests
{
    static TestResult testResult;
    
    static WeaverTests()
    {
        string name = typeof(RpcHandler.RpcDelegate).FullName;
        var weavingTask = new ModuleWeaver();
        testResult = weavingTask.ExecuteTestRun("C:\\Users\\aiden\\OneDrive\\Documentos\\GitHub\\Test-Networking-Stuff\\Package\\FodyTest\\bin\\Debug\\net6.0\\FodyTest.dll");
    }
    
    [Fact]
    public void ValidateIsInjected()
    {
        NetworkManager networkManager = new NetworkManager();
        networkManager.Init(new testTransport());
        
        testResult.Assembly.GetType("FodyTest.Class1").GetMethod("test").Invoke(null, new []{(object)2,(object)5,(object)10});
    }
}