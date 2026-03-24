
using System.Linq.Expressions;

namespace DataHandlerLibrary.Interfaces
{
    public interface IGenericService<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(bool includeMapping);
        Task<T> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task<IEnumerable<T>> GetByConditionAsync(Expression<Func<T, bool>> expression, bool includeMapping);
        Task<string> ValidateAsync(T entity);


    }
}