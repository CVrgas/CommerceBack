using CommerceBack.Entities;
using CommerceBack.Services.Base;
using CommerceBack.UnitOfWork;

namespace CommerceBack.Services
{
	public class UserService : CrudService<User>
	{
		public UserService(ILogger<CrudService<User>> logger, IUnitOfWork unitOfWork) : base(logger, unitOfWork)
		{
		}
	}
}
