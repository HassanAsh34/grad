//using StackExchange.Redis;

namespace Grad.Application.Common.Interfaces
{
	public interface IRedisServices
	{
		public Task<bool> store(string key, string value, TimeSpan expire);
		public Task<string> get(string key);
		public Task<bool> delete(string key);

		public Task<bool> storeSerialized<T>(string key, T value, TimeSpan expire);

		public Task<T> getDeserialized<T>(string key) where T : class;

		//public Task<bool> storeAttemptsCount(string email, int count);
		//public Task<int> getAttemptsCount(string email);
	}
}

