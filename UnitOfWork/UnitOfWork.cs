using CommerceBack.Context;
using CommerceBack.Repository;
using Microsoft.EntityFrameworkCore.Storage;

namespace CommerceBack.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly MyDbContext _dbContext;
    private readonly Dictionary<Type, object> _repositories = new();
    private IDbContextTransaction? _transaction;
    private int _transactionCount;

    public UnitOfWork(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IRepository<T> Repository<T>() where T : class
    {
        if (!_repositories.ContainsKey(typeof(T)))
        {
            _repositories[typeof(T)] = new Repository<T>(_dbContext);
        }
        return (IRepository<T>)_repositories[typeof(T)];
    }
    
    public async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        if (_transactionCount == 0)
            _transaction = await _dbContext.Database.BeginTransactionAsync();
        _transactionCount++;
    }

    public async Task CommitAsync()
    {
        if (_transactionCount == 1)
        {
            await _dbContext.SaveChangesAsync();
            await _transaction!.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
        _transactionCount = Math.Max(0, _transactionCount - 1);
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
        _transactionCount = 0;
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}