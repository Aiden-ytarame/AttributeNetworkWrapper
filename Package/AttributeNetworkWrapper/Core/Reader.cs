using System;
using System.IO;

namespace AttributeNetworkWrapper.Core;

public class NetworkReader : IDisposable
{
    public readonly BinaryReader BinaryReader;

    public NetworkReader(Stream stream)
    {
        BinaryReader = new BinaryReader(stream);
    }
    
    public NetworkReader(ArraySegment<byte> data)
    {
        BinaryReader = new BinaryReader(new MemoryStream(data.Array!));
    }
    
    public void Dispose()
    {
        BinaryReader.Dispose();
        GC.SuppressFinalize(this);
    }
}

public static class NetReaderExtensions
{
    public static bool ReadBoolean(this NetworkReader reader) => reader.BinaryReader.ReadBoolean();
    
    public static byte ReadByte(this NetworkReader reader) => reader.BinaryReader.ReadByte();
    public static sbyte ReadSByte(this NetworkReader reader) => reader.BinaryReader.ReadSByte();
    public static byte[] ReadBytes(this NetworkReader reader, int count) => reader.BinaryReader.ReadBytes(count);
    
    public static short ReadInt16(this NetworkReader reader) => reader.BinaryReader.ReadInt16();
    public static ushort ReadUInt16(this NetworkReader reader) => reader.BinaryReader.ReadUInt16();
    
    public static int ReadInt32(this NetworkReader reader) => reader.BinaryReader.ReadInt32();
    public static uint ReadUInt32(this NetworkReader reader) => reader.BinaryReader.ReadUInt32();
    
    public static long ReadInt64(this NetworkReader reader) => reader.BinaryReader.ReadInt64();
    public static ulong ReadUInt64(this NetworkReader reader) => reader.BinaryReader.ReadUInt64();
    
    public static float ReadSingle(this NetworkReader reader) => reader.BinaryReader.ReadSingle();
    public static double ReadDouble(this NetworkReader reader) => reader.BinaryReader.ReadDouble();
   
    public static char ReadChar(this NetworkReader reader) => reader.BinaryReader.ReadChar();
    public static string ReadString(this NetworkReader reader) => reader.BinaryReader.ReadString();
}