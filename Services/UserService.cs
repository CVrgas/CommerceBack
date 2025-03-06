using CommerceBack.Repository;
using System.Linq.Expressions;
using CommerceBack.Common.OperationResults;
using CommerceBack.Entities;
using CommerceBack.Services.Base;
using CommerceBack.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace CommerceBack.Services
{
	public class UserService : ServiceBase<User>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<UserService> _logger;

		public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger) : base(logger, unitOfWork)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public override Task<IReturnObject<User>> Get(int id, Func<IQueryable<User>, IQueryable<User>>[]? includes = null)
		{
			includes =
			[
				query => query
					.Include(u => u.CartNavigation)
					.ThenInclude( c => c.CartProducts),
			];
			return base.Get(id, includes);
		}
		public async Task<IReturnObject<User>> CreateAsync(User entity)
		{
			try
			{
				if (entity == null) return new ReturnObject<User>().BadRequest("Invalid object");

				var exist = await _unitOfWork.Repository<User>().Exists(p => p.Username == entity.Username || p.Email == entity.Email);

				if (exist) return new ReturnObject<User>().BadRequest("Product already exists");
				
				entity.CreationDate = DateTime.UtcNow;
				entity.LastAccessDate = DateTime.UtcNow;
				entity.CartNavigation = new Cart() { User = entity};

				var createdUser = await _unitOfWork.Repository<User>().Create(entity);

				return new ReturnObject<User>().Ok(createdUser);
			}
			catch (Exception ex)
			{
				return new ReturnObject<User>().InternalError("An error occurred while creating the product");
			}
		}
		public Task<bool> ExistsAsync(Expression<Func<User, bool>> predicate)
		{
			return _unitOfWork.Repository<User>().Exists(predicate);
		}
		public async Task<IReturnObject<User>> GetAsync(int id)
		{
			try
			{
				if (id == 0) return new ReturnObject<User>().BadRequest("No id provided");
				if (_unitOfWork == null) return new ReturnObject<User>().InternalError("Data store not initialized.");
				var user = await _unitOfWork.Repository<User>().Get(id);

				if (user == null) return new ReturnObject<User>().NotFound();

				return new ReturnObject<User>().Ok(user);

			}
			catch (Exception ex)
			{
				return new ReturnObject<User>().InternalError();
			}
		}
		public async Task<IReturnObject<User>> GetAsync(Expression<Func<User, bool>> predicate)
		{
			try
			{
				var user = await _unitOfWork.Repository<User>().Get(predicate);

				if (user == null) return new ReturnObject<User>().NotFound();

				return new ReturnObject<User>().Ok(user);

			}
			catch (Exception ex)
			{
				return new ReturnObject<User>().InternalError();
			}
		}
		public async Task<IReturnObject<User>> UpdateAsync(User entity)
		{
			try
			{
				if (entity.Id == 0) return new ReturnObject<User>().BadRequest("Invalid product");

				var exist = await _unitOfWork.Repository<User>().Exists(entity.Id);

				if (!exist) return new ReturnObject<User>().NotFound();

				var udpated = await _unitOfWork.Repository<User>().Update(entity);

				return new ReturnObject<User>().Ok(udpated);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Erro while updating product {entity.Id}");
				return new ReturnObject<User>().InternalError();
			}
		}
	}
}
