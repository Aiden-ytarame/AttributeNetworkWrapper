# AttributeNetworkWrapper

This is a Fody weaver addin, which runs on build to modify the IL of the project.

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
(Server_ prefix just used for readability)

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

Now when this function gets called, it instead sends the rpc to the Server, where it then will be called with the suplied params.

It moves the code that was on DoStuff into UserCode_DoStuff, that after receiving an rpc, gets called after its params are deserialized.

## Other Examples

A function using [ClientRpc] First parameter MUST be ClientNetworkConnection, which will be the client this rpc will be sent to.

Example:
```csharp
[ClientRpc]
public static void Client_DoStuff(ClientNetworkConnection conn, int param1, bool param2) 
```

And a function using [ServerRpc] can optionally have a ClientNetworkConnection parameter that will be suplied with the client who called this rpc.

Example:
```csharp
[ServerRpc]
public static void Server_DoStuff(ClientNetworkConnection sender)
{
      Client_DoStuff(sender, 1, true);
}
```

## Sending custom data

To send and receive your own data types, you can write Extension methods for NetworkWriter/NetworkReader, like:

```csharp
public static void WriteVector2(this NetworkWriter writer, Vector2 vector) { //write }

public static Vector2 ReadVector2(this NetworkReader reader) { //read }
```

These extensions are detected while bulding and used where necessary, such as:

```csharp
[MultiRpc(Unreliable)] //Gets invoked on every client, called by server.
public static void Multi_DoStuff(Vector2 vec) 
```

