using Core.Entity;
using Core.Repository;
using Core.Services;

namespace Infrastructure.Services
{
    public class BaseService<T> : IBaseService<T> where T : EntityBase
    {
        protected readonly IRepository<T> _repository;

        protected BaseService(IRepository<T> repository)
        {
            _repository = repository;
        }

        public virtual T ObterPorId(int id)
        {
            var entity = _repository.ObterPorId(id);
            if (entity is null)
                throw new Exception("Registro não encontrado.");

            return entity;
        }

        public virtual IList<T> ObterTodos()
        {
            return _repository.ObterTodos();
        }

        public virtual T Cadastrar(T entity)
        {
            _repository.Cadastrar(entity);
            return entity;
        }

        public virtual T Alterar(T entity)
        {
            _repository.Alterar(entity);
            return entity;
        }

        public virtual void Deletar(int id)
        {
            _repository.Deletar(id);
        }
    }
}