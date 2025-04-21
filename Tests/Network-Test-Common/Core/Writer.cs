using System;
using System.IO;
using System.Text;

namespace Network_Test_Common.Core
{
    public class NetworkWriter : IDisposable
    {
        static MemoryStream _memoryStream = new MemoryStream(1024);
        public readonly BinaryWriter BinaryWriter = new BinaryWriter(_memoryStream, Encoding.UTF8, true);


        public ArraySegment<byte> GetData()
        {
            return new ArraySegment<byte>(_memoryStream.GetBuffer(), 0, (int)_memoryStream.Position);
        }

        public void Dispose()
        {
            BinaryWriter.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    public static class NetWriterExtensions
    {
        public static void Write(this NetworkWriter nw, bool value) => nw.BinaryWriter.Write(value);
    
        public static void Write(this NetworkWriter nw, byte value) => nw.BinaryWriter.Write(value);
        public static void Write(this NetworkWriter nw, sbyte value) => nw.BinaryWriter.Write(value);
        public static void Write(this NetworkWriter nw, byte[] value) => nw.BinaryWriter.Write(value);
        
        public static void Write(this NetworkWriter nw, short value) => nw.BinaryWriter.Write(value);
        public static void Write(this NetworkWriter nw, ushort value) => nw.BinaryWriter.Write(value);
    
        public static void Write(this NetworkWriter nw, int value) => nw.BinaryWriter.Write(value);
        public static void Write(this NetworkWriter nw, uint value) => nw.BinaryWriter.Write(value);
    
        public static void Write(this NetworkWriter nw, long value) => nw.BinaryWriter.Write(value);
        public static void Write(this NetworkWriter nw, ulong value) => nw.BinaryWriter.Write(value);
    
        public static void Write(this NetworkWriter nw, float value) => nw.BinaryWriter.Write(value);
        public static void Write(this NetworkWriter nw, double value) => nw.BinaryWriter.Write(value);
    
        public static void Write(this NetworkWriter nw, char value) => nw.BinaryWriter.Write(value);
        public static void Write(this NetworkWriter nw, string value) => nw.BinaryWriter.Write(value);
    }
}
