using CommerceBack.Common.OperationResults;
using CommerceBack.UnitOfWork;

namespace CommerceBack.Services.Base.UpdateService;

public class UpdateService<T> : IUpdateService<T> where T : class
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateService<T>> _logger;

    protected UpdateService(IUnitOfWork unitOfWork, ILogger<UpdateService<T>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public virtual async Task<IReturnObject<T>> Update(T entity)
    {
        try
        {
            var createdEntity = await _unitOfWork.Repository<T>().Update(entity);

            return new ReturnObject<T>().Ok(createdEntity);
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
    
    private async Task<bool> Exist(int id)
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
    
    private object? GetPrimaryKeyValue(T entity)
    {
        var keyProperty = typeof(T).GetProperties()
            .FirstOrDefault(p => Attribute.IsDefined(p, typeof(System.ComponentModel.DataAnnotations.KeyAttribute)));

        return keyProperty?.GetValue(entity);
    }
}