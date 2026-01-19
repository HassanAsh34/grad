using grad.DTO;
using grad.Model;

namespace grad.Interfaces
{
	public interface ITokenServices
	{
		public string generateAccessToken(User user, bool reset = false);

		public string generateRefreshToken();

		//private Task<object> validateToken(string token);

		public Task<bool> blacklistToken(string token);

		public Task<bool> IsTokenBlacklisted(string token);

		public Task<AccessTokenDto> getTokenInfo(string token);
		//public string getUID(string token);
	}
}
