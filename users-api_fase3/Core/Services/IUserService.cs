using Core.Entity;
using Core.Input;

namespace Core.Services
{
    public interface IUserService : IBaseService<User>
    {
        Task<User> CadastrarAsync(UserInput input);
    }
}