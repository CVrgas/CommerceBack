using System.Linq.Expressions;
using CommerceBack.Common;
using CommerceBack.Common.OperationResults;
using CommerceBack.Repository;
using CommerceBack.UnitOfWork;

namespace CommerceBack.Services.Base.ReadService;

public class ReadService<T> : IReadService<T> where T : class
{
    private readonly ILogger<ReadService<T>> _logger;
    private readonly IUnitOfWork _unitOfWork;

    protected ReadService(ILogger<ReadService<T>> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
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
    public virtual async Task<IReturnObject<IEnumerable<TProjection>>> GetAll<TProjection>(Expression<Func<T, TProjection>> selector, Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
    {
        try
        {
            var objects = await _unitOfWork.Repository<T>().GetAll(selector,predicate, orderBy);
            return new ReturnObject<IEnumerable<TProjection>>().Ok(objects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new ReturnObject<IEnumerable<TProjection>>().InternalError();
        }
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
    public virtual async Task<IReturnObject<TProjection>> GetById<TProjection>(int id, Expression<Func<T, TProjection>> selector)
    {
        try
        {
            var entity = await _unitOfWork.Repository<T>().GetById(id, selector);
            return entity == null ? new ReturnObject<TProjection>().NotFound() : new ReturnObject<TProjection>().Ok(entity: entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new ReturnObject<TProjection>().InternalError();
        }
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
    public virtual async Task<IReturnObject<TProjection?>> Get<TProjection>(Expression<Func<T, bool>> predicate, Expression<Func<T, TProjection>> selector)
    {
        try
        {
            var result = await _unitOfWork.Repository<T>().Get(predicate, selector);
            if (result == null) return new ReturnObject<TProjection?>().NotFound();
            return new ReturnObject<TProjection?>().Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error");
            return new ReturnObject<TProjection?>().InternalError();
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
    public async Task<IReturnObject<PaginatedResponse<TProjection>>> GetPaginated<TProjection>(int pageIndex, int pageSize, 
        Expression<Func<T, TProjection>> selector, Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
    {
        try
        {
            var paginated = await _unitOfWork.Repository<T>().GetPaginated<TProjection>(pageIndex, pageSize, selector, predicate, orderBy);
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
}