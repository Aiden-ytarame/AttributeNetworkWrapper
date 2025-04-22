using AttributeNetworkWrapper.Core;
using Console = System.Console;

namespace FodyTest;

public class Class1
{
    [ServerRpc]
    public static void test(int param, int param2, long longParam)
    {
        Console.Write("hihhihi stuff");
    }

    [ClientRpc]
    public static void TakaDamagge(ClientNetworkConnection conn)
    {
        //siht here
    }
   
}