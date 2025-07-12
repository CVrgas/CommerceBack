using System.Linq.Expressions;
using CommerceBack.Common;

namespace CommerceBack.Repository
{
    public interface IRepository<T> : IReadRepository<T> where T : class
    {
        Task<T?> Create(T entity);

        Task<IEnumerable<T>> CreateRange(IEnumerable<T> entities);

        Task<T> Update(T entity);

        Task<IEnumerable<T>> UpdateRange(IEnumerable<T> entities);

        Task Delete(int id);

        Task Delete(T entity);

        Task Delete(IEnumerable<T> entities);

        Task DeleteRange(IEnumerable<T> entities);

        Task<T?> FindOrCreateAsync(Expression<Func<T, bool>> predicate, T newEntity);
	}
}
