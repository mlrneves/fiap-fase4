using Core.Entity;
using Core.Events;
using Core.Input;
using Core.Repository;
using Core.Services;
using Infrastructure.CrossCutting.Correlation;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Infrastructure.Services
{
    public class UserService : BaseService<User>, IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IIntegrationEventPublisher _integrationEventPublisher;
        private readonly ICorrelationIdGenerator _correlationIdGenerator;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            IIntegrationEventPublisher integrationEventPublisher,
            ICorrelationIdGenerator correlationIdGenerator,
            ILogger<UserService> logger)
            : base(userRepository)
        {
            _userRepository = userRepository;
            _integrationEventPublisher = integrationEventPublisher;
            _correlationIdGenerator = correlationIdGenerator;
            _logger = logger;
        }

        public async Task<User> CadastrarAsync(UserInput input)
        {
            _logger.LogInformation("Iniciando cadastro de usuário. Email={Email}", input.Email);

            ValidarEmail(input.Email);
            ValidarSenha(input.Password);

            var user = new User
            {
                Email = input.Email,
                Name = input.Name,
                Password = input.Password,
                Role = input.Role
            };

            var createdUser = base.Cadastrar(user);

            var correlationId = _correlationIdGenerator.Get();

            var userRegisteredEvent = new UserRegisteredEvent
            {
                UserId = createdUser.Id,
                Name = createdUser.Name,
                Email = createdUser.Email,
                Role = createdUser.Role.ToString(),
                CreatedAt = createdUser.CreatedAt,
                CorrelationId = correlationId
            };

            await _integrationEventPublisher.PublishAsync(userRegisteredEvent);

            _logger.LogInformation(
                "Usuário cadastrado com sucesso. UserId={UserId} Email={Email} CorrelationId={CorrelationId}",
                createdUser.Id,
                createdUser.Email,
                correlationId);

            return createdUser;
        }

        private void ValidarEmail(string email)
        {
            try
            {
                var _ = new MailAddress(email);
            }
            catch
            {
                _logger.LogWarning("Tentativa de cadastro com e-mail inválido. Email={Email}", email);
                throw new Exception("Formato de e-mail inválido.");
            }

            if (_userRepository.GetByEmail(email) != null)
            {
                _logger.LogWarning("Tentativa de cadastro com e-mail já existente. Email={Email}", email);
                throw new Exception("E-mail já cadastrado.");
            }
        }

        private static void ValidarSenha(string senha)
        {
            var regex = new Regex(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$");

            if (!regex.IsMatch(senha))
                throw new Exception("Senha deve ter no mínimo 8 caracteres, contendo letras, números e caractere especial.");
        }
    }
}