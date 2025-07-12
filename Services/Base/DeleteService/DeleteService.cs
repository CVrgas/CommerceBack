using CommerceBack.Common.OperationResults;
using CommerceBack.UnitOfWork;

namespace CommerceBack.Services.Base.DeleteService;

public class DeleteService<T> : IDeleteService<T> where T : class
{
    private readonly ILogger<DeleteService<T>> _logger;
    private readonly IUnitOfWork _unitOfWork;

    protected DeleteService(IUnitOfWork unitOfWork, ILogger<DeleteService<T>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
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
    
    private static object? GetPrimaryKeyValue(T entity)
    {
        var keyProperty = typeof(T).GetProperties()
            .FirstOrDefault(p => Attribute.IsDefined(p, typeof(System.ComponentModel.DataAnnotations.KeyAttribute)));

        return keyProperty?.GetValue(entity);
    }
}