using Microsoft.Extensions.Options;
using MongoDB.Driver;
using grad.Model;

namespace grad.Data
{
	public class MongoDBContext
	{
		private readonly IMongoDatabase _database;

		public MongoDBContext(IMongoClient client, IOptions<MongoDBSettings>options) 
		{
			_database = client.GetDatabase(options.Value.DB_Name) ?? throw new ArgumentNullException(nameof(client));
		}

		public IMongoCollection<SubjectContent> Subjects => _database.GetCollection<SubjectContent>("Subjects");
		public IMongoCollection<StudentProgress> StudentProgress => _database.GetCollection<StudentProgress>("StudentProgress");
	}
}
