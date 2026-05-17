namespace Core.Input
{
    public class LoginDTO
    {
        public required string Token { get; set; }
        public required DateTime Expiration { get; set; }
        public required string Role { get; set; }
    }
}
