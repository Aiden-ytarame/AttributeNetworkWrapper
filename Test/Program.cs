using Test_Networking_Stuff.Attributes;

namespace Test;

class Program
{
    static void Main(string[] args)
    {
        tests.test1();
    }
}

public class tests
{
    [ServerRpc]
    public static void test1()
    {
        
        Console.WriteLine("Hello, World! Again");
        Console.ReadLine();
    }
}