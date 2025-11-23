using StackExchange.Redis;

namespace grad.Interfaces
{
	public interface IRedisServices
	{
		public Task<bool> store(string key, string value, TimeSpan expire);
		public Task<string> get(string key);
		public Task<bool> delete(string key);

		//public Task<bool> storeAttemptsCount(string email, int count);
		//public Task<int> getAttemptsCount(string email);
	}
}

