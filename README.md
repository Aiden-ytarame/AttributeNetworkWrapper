# AttributeNetworkWrapper

This is a Fody addin, which runs on build to modify the IL of the project.

## Use

This calls a bunch of abstract functions that you, the user must override to handle sending messages, and calling certain functions on receive.

Classes that you must override is the NetworkManager and Transport.

You call rpcs by using either Server/Client/Multi Rpc Attribute:
```csharp
[ServerRpc(Reliable)]
public static void Server_DoStuff(int param1, bool param2)
{

}
```

on build, this gets compiled to:

```csharp
[ServerRpc(Reliable)]
public static void Server_DoStuff(int param1, bool param2)
{
      if (NetworkManager.Instance == null) return;

      using NetworkWriter writer = new NetworkWriter();
      writer.WritesShort(FunctionHash); //defined on build
      writer.WriteInt(param1);
      writer.WriteBool(Param2);
      NetworkManager.Instance.SendRpcToServer(writer);
}
```

It moves the code that was on DoStuff into UserCode_DoStuff, that after receiving an rpc, gets called after its params are deserialized.


