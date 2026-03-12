using Grad.Application.Auth.DTOs;
using Grad.Domain.Model;

namespace Grad.Application.Common.Interfaces
{
	public interface ITokenServices
	{
		public string generateAccessToken(User user, bool reset = false);

		public Task<string> generateRefreshToken(User user,CancellationToken cancellationToken = default);

		//private Task<object> validateToken(string token);

		public Task<bool> blacklistToken(string token, int remainingTime = 30);

		public Task<bool> IsTokenBlacklisted(string token);

		public Task<bool> RevokeRefreshToken(string RefreshToken, CancellationToken cancellationToken = default);

		public Task<RefreshTokenDTO> getTokenInfo(string token,bool refresh,CancellationToken cancellationToken = default);

		//public Task<object> getTokenInfo(string token);
		//public string getUID(string token);
	}
}
