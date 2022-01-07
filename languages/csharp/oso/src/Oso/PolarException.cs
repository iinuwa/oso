namespace Oso;

public class PolarException : Exception
{
    public PolarException() { }

    public PolarException(string? message) : base(message) { }

    public PolarException(string? message, Exception? inner) : base(message, inner) { }
}