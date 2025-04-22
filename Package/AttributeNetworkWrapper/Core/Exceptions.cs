using System;

namespace AttributeNetworkWrapper.Core;

public class NullServerException : Exception
{
    public NullServerException(string message) : base(message) { }
}