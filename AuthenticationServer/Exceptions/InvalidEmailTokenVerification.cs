namespace AuthenticationServer.Exceptions;

public class InvalidEmailVerificationTokenException : Exception
{
    public InvalidEmailVerificationTokenException()
        : base("The email verification token is invalid.")
    {
    }
}