using Core.Input;
using Core.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services
{
    public class AuthService(IUserRepository userRepository, IConfiguration configuration) : IAuthService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IConfiguration _configuration = configuration;

        public LoginDTO Authenticate(LoginInput request)
        {
            var user = _userRepository.GetByEmail(request.Email);

            if (user is null || user.Password != request.Password)
                throw new UnauthorizedAccessException("Usuário ou senha inválidos.");

            var expirationMinutesStr = _configuration["Jwt:ExpirationMinutes"];
            DateTime dataExpiracao = DateTime.UtcNow.AddMinutes(string.IsNullOrEmpty(expirationMinutesStr) ? 30 : int.Parse(expirationMinutesStr));
            var token = GenerateToken(user, dataExpiracao);

            return new LoginDTO
            {
                Token = token,
                Expiration = dataExpiracao,
                Role = user.Role.ToString()
            };
        }

        private string GenerateToken(UserDto user, DateTime dataExpiracao)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: null,
                claims: claims,
                expires: dataExpiracao,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
