using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Grad_Structured.Domain.Model;
using Grad_Structured.Infrastructure.Persistence.Configurations;

namespace Grad_Structured.Infrastructure.Persistence
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
