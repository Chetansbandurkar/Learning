namespace AuthenticationServer.DTO
{
    public class LogoutRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
