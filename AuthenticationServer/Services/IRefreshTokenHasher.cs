namespace AuthenticationServer.Services
{
    public interface IRefreshTokenHasher
    {
        string Hash(string refreshToken);
    }
}
