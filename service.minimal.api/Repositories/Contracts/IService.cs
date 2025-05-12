using System.Linq.Expressions;

namespace service.minimal.api.Repositories.Contracts
{
    public interface IService<Type>
    {
        Task<List<Type>> GetAllAsync();
        Task<Type> GetByExpresionAsync(Expression<Func<Type, bool>> expression);
        Task<Type> AddNewEntityAsync(Type entity);
        Task<Type> UpdateEntityAsync(Type entity);
        Task<bool> DeleteEntityAsync(Expression<Func<Type, bool>> expression);
    }
}
