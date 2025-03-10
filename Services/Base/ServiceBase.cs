using System.Linq.Expressions;
using CommerceBack.Common;
using CommerceBack.Common.OperationResults;
using CommerceBack.UnitOfWork;

namespace CommerceBack.Services.Base;

public abstract class ServiceBase<T> : IServiceBase<T>
    where T : class
{
    private readonly ILogger<ServiceBase<T>> _logger;
    private readonly IUnitOfWork _unitOfWork;

    protected ServiceBase(ILogger<ServiceBase<T>> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }
    

    public virtual async Task<IReturnObject<T>> GetById(int id, Func<IQueryable<T>, IQueryable<T>>[]? includes = null!)
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

    public virtual async Task<IReturnObject<T>> Get(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>>[]? includes = null!)
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

    public virtual async Task<IReturnObject<IEnumerable<T>>> GetAll(Expression<Func<T, bool>>? predicate = null, Expression<Func<T, object>>? orderBy = null, Func<IQueryable<T>, IQueryable<T>>[]? includes = null)
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
    
    public virtual async Task<IReturnObject<PaginatedResponse<T>>> GetPaginated(int pageIndex, int pageSize,
        Expression<Func<T, bool>>? predicate = null, Expression<Func<T, object>>? orderBy = null,
        Func<IQueryable<T>, IQueryable<T>>[]? includes = null)
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
            _logger.LogError(ex, "Error while checking if entity exists");
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
            _logger.LogError(ex, $"Error while checking if entity exists with id {id}");
            throw;
        }
    }
    
    public virtual async Task<IReturnObject<T>> Create(T entity)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var exists = await this.Exist(e => e.Equals(entity));
            if (exists)
            {
                return new ReturnObject<T>().BadRequest("Entity already exists");
            }

            var createdEntity = await _unitOfWork.Repository<T>().Create(entity);
            await _unitOfWork.CommitAsync();

            return createdEntity != null
                ? new ReturnObject<T>().Ok(createdEntity)
                : new ReturnObject<T>().BadRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating the entity");
            await _unitOfWork.RollbackAsync();
            return new ReturnObject<T>().InternalError("An error occurred while creating the entity");
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
            await _unitOfWork.Repository<T>().Delete(entity);
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
}