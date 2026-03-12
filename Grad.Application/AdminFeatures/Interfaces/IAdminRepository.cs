using Grad.Application.Common.Interfaces;
using Grad.Domain.Model;

namespace Grad.Application.AdminFeatures.Interfaces
{
	public interface IAdminRepository  : IRepository
	{
		Task<User> getUserWithRefreshToken(Guid uid, CancellationToken cancellation = default);
	}
}
