using Core.Entity;

namespace Core.Services
{
    public interface IBaseService<T> where T : EntityBase
    {
        T ObterPorId(int id);
        IList<T> ObterTodos();
        T Cadastrar(T entity);
        T Alterar(T entity);
        void Deletar(int id);
    }
}