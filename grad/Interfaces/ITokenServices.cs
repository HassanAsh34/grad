using grad.Model;

namespace grad.Interfaces
{
	public interface ITokenServices
	{
		public string generateAccessToken(User user, bool reset = false);

		public string generateRefreshToken();

		public Task<bool> blacklistToken(string token);

		public Task<bool> IsTokenBlacklisted(string token);

		public string getUID(string token);
	}
}
