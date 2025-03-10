using CommerceBack.Entities;
using CommerceBack.Services.Base;
using CommerceBack.UnitOfWork;

namespace CommerceBack.Services
{
	public class UserService : ServiceBase<User>
	{
		public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger) : base(logger, unitOfWork)
		{
		}
	}
}
