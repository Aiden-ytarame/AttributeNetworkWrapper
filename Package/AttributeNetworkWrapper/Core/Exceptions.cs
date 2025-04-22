using System;

namespace AttributeNetworkWrapper.Core;

/// <summary>
/// Might remove
/// </summary>
public class NullServerException : Exception
{
    public NullServerException(string message) : base(message) { }
}