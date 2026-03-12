
using Grad.Application.Common.Interfaces;
using Grad.Domain.Model;

namespace Grad.Application.Auth.Interfaces
{
	public interface IAuthRepository : IRepository
	{
		Task<User> getUserWithRefreshToken(Guid ?uid, CancellationToken cancellation = default);

		Task<bool> IsUserExist(string? email = "", Guid? uid = null, CancellationToken CT = default);

		Task<string> getEmail(string email, CancellationToken CT = default);
	}
}
