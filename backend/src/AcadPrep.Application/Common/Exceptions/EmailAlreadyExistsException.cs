namespace Application.Common.Exceptions;

public class EmailAlreadyExistsException : Exception
{
    public EmailAlreadyExistsException()
        : base("An account with this email address already exists.")
    {
    }

    public EmailAlreadyExistsException(string email)
        : base($"An account with email '{email}' already exists.")
    {
    }
}
