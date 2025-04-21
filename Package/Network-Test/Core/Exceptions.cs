using System;

namespace Network_Test.Core;

public class NullServerException : Exception
{
    public NullServerException(string message) : base(message) { }
}