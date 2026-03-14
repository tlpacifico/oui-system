namespace shs.Application.Exceptions;

public sealed class OuiException : Exception
{
    public OuiException(string requestName, Exception? innerException = null)
        : base($"Unhandled exception for {requestName}", innerException)
    {
        RequestName = requestName;
    }

    public string RequestName { get; }
}
