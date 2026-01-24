using grad.DTO;
using grad.Model;

namespace grad.Interfaces
{
	public interface ITokenServices
	{
		public string generateAccessToken(User user, bool reset = false);

		public Task<string> generateRefreshToken(User user,CancellationToken cancellationToken);

		//private Task<object> validateToken(string token);

		public Task<bool> blacklistToken(string token);

		public Task<bool> IsTokenBlacklisted(string token);

		public Task<bool> RevokeRefreshToken(string RefreshToken, CancellationToken cancellationToken);

		public Task<RefreshTokenDTO> getTokenInfo(string token, CancellationToken cancellationToken = default);

		//public Task<object> getTokenInfo(string token);
		//public string getUID(string token);
	}
}
