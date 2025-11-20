using System.Net;
using BCrypt.Net;
using grad.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using StackExchange.Redis;
using StackExchange.Redis.Maintenance;
using StackExchange.Redis.Profiling;
using static System.Net.WebRequestMethods;

namespace grad.Services
{
	public class RedisServices : IRedisServices
	{
		private readonly StackExchange.Redis.IDatabase _redis;
		private readonly bool _enabled;
		public RedisServices(IConnectionMultiplexer connection)
		{
			if (connection is NoOpConnectionMultiplexer)
			{
				_enabled = false;
				_redis = null!;
			}
			else
			{
				_redis = connection.GetDatabase();
				_enabled = true;
			}
		}

		public async Task<bool> store(string key,string value, TimeSpan expire)
		{
			//string keyHashed = BCrypt.Net.BCrypt.HashPassword(key);
			if(!_enabled)
				return false;
			var res = get(key);
			if (res != null)
				await delete(key);
			return await _redis.StringSetAsync(key, value, expire);
		}

		public async Task<string> get(string key)
		{
			//string keyHashed = BCrypt.Net.BCrypt.HashPassword(key);
			if (!_enabled)
				return null;
			string stored = await _redis.StringGetAsync(key);
			return stored;
		}


		public async Task<bool> delete(string key)
		{
			//string keyHashed = BCrypt.Net.BCrypt.HashPassword(key);
			if (!_enabled)
				return false;
			return await _redis.KeyDeleteAsync(key);
		}


		//public async Task<bool> storeAttemptsCount(string email, int count)
		//{
		//	return await _redis.StringSetAsync(email + "_attempts", count, TimeSpan.FromHours(1));
		//}
		//public async Task<int> getAttemptsCount(string email)
		//{
		//	return Convert.ToInt32(await _redis.StringGetAsync(email + "_attempts"));
		//}

		
	}
	public sealed class NoOpConnectionMultiplexer : IConnectionMultiplexer
	{
		public static readonly IConnectionMultiplexer Instance = new NoOpConnectionMultiplexer();

		public event EventHandler<RedisErrorEventArgs> ErrorMessage;
		public event EventHandler<ConnectionFailedEventArgs> ConnectionFailed;
		public event EventHandler<InternalErrorEventArgs> InternalError;
		public event EventHandler<ConnectionFailedEventArgs> ConnectionRestored;
		public event EventHandler<EndPointEventArgs> ConfigurationChanged;
		public event EventHandler<EndPointEventArgs> ConfigurationChangedBroadcast;
		public event EventHandler<ServerMaintenanceEvent> ServerMaintenanceEvent;
		public event EventHandler<HashSlotMovedEventArgs> HashSlotMoved;

		private NoOpConnectionMultiplexer() { }

		public void Dispose() { }

		public Microsoft.EntityFrameworkCore.Storage.IDatabase GetDatabase(int db = -1, object asyncState = null)
			=> throw new InvalidOperationException("Redis is disabled — no database available.");

		public IServer GetServer(string host, int port, object asyncState = null)
			=> throw new InvalidOperationException("Redis is disabled — no server available.");

		public string ClientName => "NoOpConnectionMultiplexer";
		public string Configuration => "Redis disabled";
		public bool IsConnected => false;
		public bool IsConnecting => false;

		public int TimeoutMilliseconds => throw new NotImplementedException();

		public long OperationCount => throw new NotImplementedException();

		public bool PreserveAsyncOrder { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public bool IncludeDetailInExceptions { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public int StormLogThreshold { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		// All other members can throw or return defaults
		public void Close(bool allowCommandsToComplete = true) { }
		public void CloseAsync(bool allowCommandsToComplete = true) { }
		public void RegisterProfiler(Func<ProfilingSession> profilingSessionProvider) { }
		public void ResetStormLog() { }
		public void Wait(Task task) { }
		public T Wait<T>(Task<T> task) => default!;
		public void WaitAll(params Task[] tasks) { }

		public ServerCounters GetCounters()
		{
			throw new NotImplementedException();
		}

		public EndPoint[] GetEndPoints(bool configuredOnly = false)
		{
			throw new NotImplementedException();
		}

		public int HashSlot(RedisKey key)
		{
			throw new NotImplementedException();
		}

		public ISubscriber GetSubscriber(object? asyncState = null)
		{
			throw new NotImplementedException();
		}

		StackExchange.Redis.IDatabase IConnectionMultiplexer.GetDatabase(int db, object? asyncState)
		{
			throw new NotImplementedException();
		}

		public IServer GetServer(string hostAndPort, object? asyncState = null)
		{
			throw new NotImplementedException();
		}

		public IServer GetServer(IPAddress host, int port)
		{
			throw new NotImplementedException();
		}

		public IServer GetServer(EndPoint endpoint, object? asyncState = null)
		{
			throw new NotImplementedException();
		}

		public IServer GetServer(RedisKey key, object? asyncState = null, CommandFlags flags = CommandFlags.None)
		{
			throw new NotImplementedException();
		}

		public IServer[] GetServers()
		{
			throw new NotImplementedException();
		}

		public Task<bool> ConfigureAsync(TextWriter? log = null)
		{
			throw new NotImplementedException();
		}

		public bool Configure(TextWriter? log = null)
		{
			throw new NotImplementedException();
		}

		public string GetStatus()
		{
			throw new NotImplementedException();
		}

		public void GetStatus(TextWriter log)
		{
			throw new NotImplementedException();
		}

		Task IConnectionMultiplexer.CloseAsync(bool allowCommandsToComplete)
		{
			throw new NotImplementedException();
		}

		public string? GetStormLog()
		{
			throw new NotImplementedException();
		}

		public long PublishReconfigure(CommandFlags flags = CommandFlags.None)
		{
			throw new NotImplementedException();
		}

		public Task<long> PublishReconfigureAsync(CommandFlags flags = CommandFlags.None)
		{
			throw new NotImplementedException();
		}

		public int GetHashSlot(RedisKey key)
		{
			throw new NotImplementedException();
		}

		public void ExportConfiguration(Stream destination, ExportOptions options = (ExportOptions)(-1))
		{
			throw new NotImplementedException();
		}

		public void AddLibraryNameSuffix(string suffix)
		{
			throw new NotImplementedException();
		}

		public ValueTask DisposeAsync()
		{
			throw new NotImplementedException();
		}

		// Other interface members omitted — not needed for your case
	}
}
