using System.Linq.Expressions;
using CommerceBack.Common;
using CommerceBack.Common.OperationResults;
using CommerceBack.Services.Base.CreateService;
using CommerceBack.UnitOfWork;

namespace CommerceBack.Services.Base;

public class CrudService<T> : IReadService<T>, ICreateService<T>, IUpdateService<T>, IDeleteService<T> where T : class
{
    private readonly ILogger<CrudService<T>> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CrudService(ILogger<CrudService<T>> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public virtual async Task<IReturnObject<T>> GetById(int id, Func<IQueryable<T>, IQueryable<T>>? includes = null!)
    {
        try
        {
            var entity = await _unitOfWork.Repository<T>().GetById(id, includes);
            return entity == null ? new ReturnObject<T>().NotFound() : new ReturnObject<T>().Ok(entity: entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new ReturnObject<T>().InternalError();
        }
    }

    public Task<IReturnObject<TProjection>> GetById<TProjection>(int id, Expression<Func<T, TProjection>> selector)
    {
        throw new NotImplementedException();
    }

    public virtual async Task<IReturnObject<T>> Find(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>>? includes = null!)
    {
        try
        {
            var entity = await _unitOfWork.Repository<T>().Get(predicate, includes);
            return entity == null ? new ReturnObject<T>().NotFound() : new ReturnObject<T>().Ok(entity: entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new ReturnObject<T>().InternalError();
        }
    }

    public virtual async Task<IReturnObject<IEnumerable<T>>> GetAll(Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, Func<IQueryable<T>, IQueryable<T>>? includes = null)
    {
        try
        {
            var objects = await _unitOfWork.Repository<T>().GetAll(predicate, orderBy, includes);
            return new ReturnObject<IEnumerable<T>>().Ok(objects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new ReturnObject<IEnumerable<T>>().InternalError();
        }
    }

    public async Task<IReturnObject<IEnumerable<TProjection>>> GetAll<TProjection>(Expression<Func<T, TProjection>> selector, Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
    {
        try
        {
            var objects = await _unitOfWork.Repository<T>().GetAll(selector, predicate, orderBy);
            return new ReturnObject<IEnumerable<TProjection>>().Ok(objects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new ReturnObject<IEnumerable<TProjection>>().InternalError();
        }
    }

    public virtual async Task<IReturnObject<PaginatedResponse<T>>> GetPaginated(int pageIndex, int pageSize,
        Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IQueryable<T>>? includes = null)
    {
        try
        {
            var paginated = await _unitOfWork.Repository<T>().GetPaginated(pageIndex, pageSize, predicate, orderBy, includes);
            return new ReturnObject<PaginatedResponse<T>>().Ok(paginated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new ReturnObject<PaginatedResponse<T>>().InternalError();
        }
    }

    public virtual async Task<IReturnObject<PaginatedResponse<TProjection>>> GetPaginated<TProjection>(int pageIndex, int pageSize, Expression<Func<T, TProjection>> selector, Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
    {
        try
        {
            var paginated = await _unitOfWork.Repository<T>().GetPaginated(pageIndex, pageSize,selector, predicate, orderBy);
            return new ReturnObject<PaginatedResponse<TProjection>>().Ok(paginated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new ReturnObject<PaginatedResponse<TProjection>>().InternalError();
        }
    }

    public virtual IReturnObject<int> GetCount(Expression<Func<T, bool>>? predicate = null)
    {
        try
        {
            predicate ??= _ => true;
            var count = _unitOfWork.Repository<T>().Count(predicate);
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
            return await _unitOfWork.Repository<T>().Exists(predicate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error while checking if {typeof(T).Name} exists");
            throw;
        }
    }

    public virtual async Task<bool> Exist(int id)
    {
        try
        {
            return await _unitOfWork.Repository<T>().Exists(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error while checking if {typeof(T).Name} exists with id {id}");
            throw;
        }
    }
    
    public virtual async Task<IReturnObject<T>> Create(T entity)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var exists = await Exist(e => e.Equals(entity));
            if (exists)
            {
                return new ReturnObject<T>().BadRequest($"{typeof(T).Name} already exists");
            }

            var createdEntity = await _unitOfWork.Repository<T>().Create(entity);
            await _unitOfWork.CommitAsync();

            return createdEntity != null
                ? new ReturnObject<T>().Ok(createdEntity)
                : new ReturnObject<T>().BadRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while creating {typeof(T).Name}");
            await _unitOfWork.RollbackAsync();
            return new ReturnObject<T>().InternalError($"An error occurred while creating {typeof(T).Name}");
        }
    }
    
    public virtual async Task<IReturnObject<IEnumerable<T>>> BulkCreate(IEnumerable<T> entity)
    {
        try
        {
            var createdEntities = await _unitOfWork.Repository<T>().CreateRange(entity);
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
            var createdEntity = await _unitOfWork.Repository<T>().Update(entity);
            await _unitOfWork.SaveChangesAsync();
            return new ReturnObject<T>().Ok(createdEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while updating {typeof(T).Name}");
            return new ReturnObject<T>().InternalError($"An error occurred while updating {typeof(T).Name}");
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
            
            var createdEntities = await _unitOfWork.Repository<T>().CreateRange(validEntities);
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
            var entity = await _unitOfWork.Repository<T>().GetById(id);
            if (entity == null) return new ReturnObject<T>().NotFound();
            return await Delete(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while deleting {typeof(T).Name}");
            return new ReturnObject<T>().InternalError($"An error occurred while deleting {typeof(T).Name}");
        }
    }
    
    public virtual async Task<IReturnObject<T>> Delete(T entity)
    {
        try
        {
            var primaryKeyValue = GetPrimaryKeyValue(entity);
            var exists = primaryKeyValue is int id && await Exist(id);
            if (!exists) return new ReturnObject<T>().NotFound("Entity does not exist");
            await _unitOfWork.Repository<T>().Delete(entity);
            return new ReturnObject<T>().Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while deleting {typeof(T).Name}");
            return new ReturnObject<T>().InternalError($"An error occurred while deleting {typeof(T).Name}");
        }
    }
    
    public virtual async Task<IReturnObject<IEnumerable<T>>> BulkDelete(IEnumerable<T> entity)
    {
        try
        {
            await _unitOfWork.Repository<T>().DeleteRange(entity);
            return new ReturnObject<IEnumerable<T>>().Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while deleting the entities");
            return new ReturnObject<IEnumerable<T>>().InternalError("An error occurred while deleting the entities");
        }
    }
    
    private object? GetPrimaryKeyValue(T entity)
    {
        var keyProperty = typeof(T).GetProperties()
            .FirstOrDefault(p => Attribute.IsDefined(p, typeof(System.ComponentModel.DataAnnotations.KeyAttribute)));

        return keyProperty?.GetValue(entity);
    }
    
    public virtual async Task<IReturnObject<TProjection?>> Get<TProjection>(Expression<Func<T, bool>> predicate, Expression<Func<T, TProjection>> selector)
    {
        try
        {
            var result =  await _unitOfWork.Repository<T>().Get(predicate, selector);
            if(result  == null) return new ReturnObject<TProjection?>().NotFound();
            return new ReturnObject<TProjection?>().Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error");
            return new ReturnObject<TProjection?>().InternalError();
        }
    }
}