namespace StudentApiServer.DTOs
{
    public class RefreshRequest
    {
        public string RefreshToken { get; set; } = default!;
        public string Email { get; set; } = default!;
    }
}
