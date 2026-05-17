using Core.Entity;
using Core.Input;

namespace Core.Repository
{
    public interface IUserRepository : IRepository<User>
    {
        UserDto GetByEmail(string email);
        UserDto ObterUsuariosSeisMeses(int id);
    }
}
