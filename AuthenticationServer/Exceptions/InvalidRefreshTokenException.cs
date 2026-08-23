namespace AuthenticationServer.Exceptions
{
    public class InvalidRefreshTokenException : Exception
    {
        public InvalidRefreshTokenException(string refreshToken)
            : base($"The provided refresh token '{refreshToken}' is invalid.")
        {
        }
    }
}
