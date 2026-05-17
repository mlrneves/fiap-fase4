using Core.Entity;
using Core.Input;
using Core.Repository;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class UserRepository : EFRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public UserDto GetByEmail(string email)
        {
            return _context.Users
                .Where(u => u.Email == email)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    CreatedAt = u.CreatedAt,
                    Name = u.Name,
                    Email = u.Email,
                    Password = u.Password,
                    Role = u.Role
                })
                .FirstOrDefault();
        }

        public UserDto ObterUsuariosSeisMeses(int id)
        {
            var users = _context.Users
                .FirstOrDefault(u => u.Id == id) ?? throw new Exception("Esse usuário não existe.");

            return new UserDto()
            {
                Id = users.Id,
                CreatedAt = users.CreatedAt,
                Name = users.Name,
                Email = users.Email,
                Password = users.Password,
                Role = users.Role,
                //Pedidos = users.Pedidos.
                //    Where(c => c.DataCriacao >= DateTime.Now.AddMonths(-6)).
                //    Select(pedido => new PedidoDto()
                //    { 
                //        Id = pedido.Id,
                //        DataCriacao = pedido.DataCriacao,
                //        LivroId = pedido.LivroId,
                //        ClienteId = pedido.ClienteId,
                //        Livro = newLivroDto() 
                //        { 
                //            Id = pedido.Livro.Id,
                //            DataCriacao = pedido.Livro.DataCriacao,
                //            Nome = pedido.Livro.Nome,
                //            Editora = pedido.Livro.Editora
                //        }
                //    }).ToList()
            };
        }
    }
}
