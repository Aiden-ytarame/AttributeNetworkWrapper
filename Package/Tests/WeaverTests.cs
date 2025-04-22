using AttributeNetworkWrapper;
using AttributeNetworkWrapper.Core;
using AttributeNetworkWrapper.Fody;
using Fody;
using Xunit;

namespace Tests;

//todo: make proper tests
public class WeaverTests
{
    static TestResult testResult;
    
    static WeaverTests()
    {
        string name = typeof(RpcHandler.RpcDelegate).FullName;
        var weavingTask = new ModuleWeaver();
        testResult = weavingTask.ExecuteTestRun("Test-Networking-Stuff\\Package\\FodyTest\\bin\\Debug\\net6.0\\FodyTest.dll");
    }
    
    [Fact]
    public void ValidateIsInjected()
    {
        NetworkManager networkManager = new NetworkManager();
        networkManager.Init(new testTransport());

        //this throws cuz theres no NetworkManager, but still, it means in built correctly
        testResult.Assembly.GetType("FodyTest.Class1").GetMethod("test").Invoke(null, new []{(object)2,(object)5,(object)10});
    }
}
