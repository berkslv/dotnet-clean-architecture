namespace Domain.Exceptions;

public class BadRequestException : Exception
{
    public string LocalizedMessage { get; init; }

    public object[] Arguments { get; init; }

    public BadRequestException(string localizedMessage, params object[] arguments)
        : base(localizedMessage)
    {
        LocalizedMessage = localizedMessage;
        Arguments = arguments;
    }
}