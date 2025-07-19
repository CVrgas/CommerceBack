using System.Linq.Expressions;
using CommerceBack.Common.OperationResults;
using CommerceBack.UnitOfWork;

namespace CommerceBack.Services.Base.CreateService;

public class CreateService<T> : ICreateService<T> where T : class
{
    private readonly ILogger<CreateService<T>> _logger;
    private readonly IUnitOfWork _unitOfWork;

    protected CreateService(IUnitOfWork unitOfWork, ILogger<CreateService<T>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
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
    
    private async Task<bool> Exist(Expression<Func<T, bool>> predicate)
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
}