using Core.Entity;

namespace Core.Input
{
    public class UserInput
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required UserRole Role { get; set; }
    }
}