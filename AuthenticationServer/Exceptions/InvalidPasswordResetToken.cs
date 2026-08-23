namespace AuthenticationServer.Exceptions;

public class InvalidPasswordResetTokenException : Exception
{
    public InvalidPasswordResetTokenException()
        : base("The password reset token is invalid.")
    {
    }
}