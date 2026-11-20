namespace StudentApiServer.DTOs
{
    public class LogoutRequest
    {
        public string Email { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
    }
}
