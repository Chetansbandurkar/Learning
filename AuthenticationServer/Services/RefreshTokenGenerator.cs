using System.Security.Cryptography;

namespace AuthenticationServer.Services
{
    public class RefreshTokenGenerator : IRefreshTokenGenerator
    {
        public string Generate()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);

            return Convert.ToBase64String(bytes);
        }
    }
}
