using System.Linq.Expressions;
using CommerceBack.Common;
using CommerceBack.Common.OperationResults;
using CommerceBack.Repository;

namespace CommerceBack.Services.Base;

public abstract class ServiceBase<T> : IServiceBase<T>
    where T : class
{
    private readonly ILogger<ServiceBase<T>> _logger;
    private readonly IEntityStore<T> _store;

    protected ServiceBase(ILogger<ServiceBase<T>> logger, IEntityStore<T> store)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }
    

    public virtual async Task<IReturnObject<T>> Get(int id, Func<IQueryable<T>, IQueryable<T>>[] includes = null!)
    {
        try
        {
            var entity = await _store.Get(id, includes);
            return entity == null ? new ReturnObject<T>().NotFound() : new ReturnObject<T>().Ok(entity: entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new ReturnObject<T>().InternalError();
        }
    }

    public virtual async Task<IReturnObject<T>> Get(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>>[] includes = null!)
    {
        try
        {
            var entity = await _store.Get(predicate, includes);
            return entity == null ? new ReturnObject<T>().NotFound() : new ReturnObject<T>().Ok(entity: entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new ReturnObject<T>().InternalError();
        }
    }

    public virtual async Task<IReturnObject<IEnumerable<T>>> All(Expression<Func<T, bool>>? predicate = null, Expression<Func<T, object>>? orderBy = null, Func<IQueryable<T>, IQueryable<T>>[]? includes = null)
    {
        try
        {
            var objects = await _store.All(predicate, orderBy, includes);
            return new ReturnObject<IEnumerable<T>>().Ok(objects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new ReturnObject<IEnumerable<T>>().InternalError();
        }
    }
    
    public virtual async Task<IReturnObject<PaginatedResponse<T>>> Paginated(int pageIndex, int pageSize,
        Expression<Func<T, bool>>? predicate = null, Expression<Func<T, object>>? orderBy = null,
        Func<IQueryable<T>, IQueryable<T>>[]? includes = null)
    {
        try
        {
            var paginated = await _store.GetPagianted(pageIndex, pageSize, predicate, orderBy, includes);
            return new ReturnObject<PaginatedResponse<T>>().Ok(paginated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new ReturnObject<PaginatedResponse<T>>().InternalError();
        }
    }

    public virtual IReturnObject<int> Count(Expression<Func<T, bool>>? predicate = null)
    {
        try
        {
            predicate ??= _ => true;
            var count = _store.Count(predicate);
            return new ReturnObject<int>().Ok(count);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while counting entities.");
            return new ReturnObject<int>().InternalError();
        }
    }

    public virtual async Task<bool> Exist(Expression<Func<T, bool>> predicate)
    {
        try
        {
            return await _store.Exists(predicate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while checking if entity exists");
            throw;
        }
    }

    public virtual async Task<bool> Exist(int id)
    {
        try
        {
            return await _store.Exists(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error while checking if entity exists with id {id}");
            throw;
        }
    }
    

    public virtual async Task<IReturnObject<T>> Create(T entity)
    {
        try
        {
            var exists = await this.Exist(e => e.Equals(entity));
            if (exists)
            {
                return new ReturnObject<T>().BadRequest("Entity already exists");
            }

            var createdEntity = await _store.Create(entity);

            return createdEntity != null
                ? new ReturnObject<T>().Ok(createdEntity)
                : new ReturnObject<T>().BadRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating the entity");
            return new ReturnObject<T>().InternalError("An error occurred while creating the entity");
        }
    }
    

    public virtual async Task<IReturnObject<IEnumerable<T>>> BulkCreate(IEnumerable<T> entity)
    {
        try
        {
            var createdEntities = await _store.CreateRange(entity);
            return new ReturnObject<IEnumerable<T>>().Ok(createdEntities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating entities");
            return new ReturnObject<IEnumerable<T>>().InternalError("An error occurred while creating entities");
        }
    }
    

    public virtual async Task<IReturnObject<T>> Update(T entity)
    {
        try
        {
            var createdEntity = await _store.Update(entity);

            return createdEntity != null ? new ReturnObject<T>().Ok(createdEntity) : new ReturnObject<T>().BadRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating the entity");
            return new ReturnObject<T>().InternalError("An error occurred while updating the entity");
        }
    }
    
    
    public virtual async Task<IReturnObject<IEnumerable<T>>> BulkUpdate(IEnumerable<T> entity)
    {
        try
        {
            var tasks = entity.Select(async e =>
            {
                var primaryKeyValue = GetPrimaryKeyValue(e);
                var exists = primaryKeyValue is int id && await Exist(id);
                return (Entity: e, Exists: exists);
            });
            
            var results = await Task.WhenAll(tasks);
            
            var validEntities = results
                .Where(result => result.Exists)
                .Select(result => result.Entity);
            
            var createdEntities = await _store.CreateRange(validEntities);
            return new ReturnObject<IEnumerable<T>>().Ok(createdEntities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating the entities");
            return new ReturnObject<IEnumerable<T>>().InternalError("An error occurred while updating");
        }
    }

    public virtual async Task<IReturnObject<T>> Delete(int id)
    {
        try
        {
            var entity = await _store.Get(id);
            if (entity == null) return new ReturnObject<T>().NotFound();
            return await Delete(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting the entity");
            return new ReturnObject<T>().InternalError("An error occurred while deleting the entity");
        }
    }
    
    public virtual async Task<IReturnObject<T>> Delete(T entity)
    {
        try
        {
            var primaryKeyValue = GetPrimaryKeyValue(entity);
            var exists = primaryKeyValue is int id && await Exist(id);
            if (!exists) return new ReturnObject<T>().NotFound("Entity does not exist");
            await _store.Delete(entity);
            return new ReturnObject<T>().Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting the entity");
            return new ReturnObject<T>().InternalError("An error occurred while deleting the entity");
        }
    }
    
    
    public virtual async Task<IReturnObject<IEnumerable<T>>> BulkDelete(IEnumerable<T> entity)
    {
        try
        {
            await _store.DeleteRange(entity);
            return new ReturnObject<IEnumerable<T>>().Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while deleting the entities");
            return new ReturnObject<IEnumerable<T>>().InternalError("An error occurred while deleting the entities");
        }
    }

    public virtual IQueryable<T> Query()
    {
        return _store.Querable();
    }
    
    private object? GetPrimaryKeyValue(T entity)
    {
        var keyProperty = typeof(T).GetProperties()
            .FirstOrDefault(p => Attribute.IsDefined(p, typeof(System.ComponentModel.DataAnnotations.KeyAttribute)));

        return keyProperty?.GetValue(entity);
    }
}