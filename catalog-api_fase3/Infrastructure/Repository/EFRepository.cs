using Core.Entity;
using Core.Repository;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class EFRepository<T> : IRepository<T> where T : EntityBase
    {
        protected ApplicationDbContext _context;
        protected DbSet<T> _dbSet;

        public EFRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public void Alterar(T entidade)
        {
            entidade.UpdatedAt = DateTime.Now;
            _dbSet.Update(entidade);
            _context.SaveChanges();
        }

        public void Cadastrar(T entidade)
        {
            entidade.CreatedAt = DateTime.Now;
            _dbSet.Add(entidade);
            _context.SaveChanges();
        }

        public void Deletar(int id)
        {
            var entidade = ObterPorId(id);
            if (entidade is null)
                throw new Exception("Registro não encontrado.");
            
            _dbSet.Remove(entidade);
            _context.SaveChanges();
        }

        public T ObterPorId(int id) => _dbSet.FirstOrDefault(entity => entity.Id == id);

        public IList<T> ObterTodos() => _dbSet.ToList();
    }
}
