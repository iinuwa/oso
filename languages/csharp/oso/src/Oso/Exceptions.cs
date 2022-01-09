namespace Oso;

internal static class Exceptions
{

}

public class OsoException : Exception
{
    public OsoException() { }

    public OsoException(string? message) : base(message) { }

    public OsoException(string? message, Exception? inner) : base(message, inner) { }
}

public class PolarRuntimeException : OsoException
{
    public PolarRuntimeException() { }

    public PolarRuntimeException(string? message) : base(message) { }

    public PolarRuntimeException(string? message, Exception? inner) : base(message, inner) { }
}

// TODO: Do we need so many exception types?
public class InvalidAttributeException : PolarRuntimeException
{
    public InvalidAttributeException(string? message) : base(message) { }

    public InvalidAttributeException(string className, string attrName) : base($"Invalid attribute `{attrName}` on class {className}") { }

}

public class InvalidCallException : PolarRuntimeException
{
    public InvalidCallException() { }
    public InvalidCallException(string? message) : base(message) { }
    public InvalidCallException(string? message, Exception? inner) : base(message, inner) { }
    public InvalidCallException(string className, string callName, params Type[] argTypes) : base($"Invalid call {callName} on class {className}, with argument types `{argTypes}`") { }
}

public class InvalidConstructorException : PolarRuntimeException
{
    public InvalidConstructorException(string? message) : base(message) { }
}
