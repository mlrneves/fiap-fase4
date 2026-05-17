using Core.Input;

namespace Core.Repository
{
    public interface IAuthService
    {
        LoginDTO Authenticate(LoginInput request);
    }
}