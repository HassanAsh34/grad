using Grad.Domain.Model;
using Grad.Infrastructure.Persistence.Configurations;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace  Grad.Infrastructure.Persistence
{
	public class MongoDBContext
	{
		private readonly IMongoDatabase _database;

		public MongoDBContext(IMongoClient client, IOptions<MongoDBSettings>options) 
		{
			_database = client.GetDatabase(options.Value.DB_Name) ?? throw new ArgumentNullException(nameof(client));
			ConfigureMappings();
		}
		

		public IMongoCollection<SubjectContent> Subjects => _database.GetCollection<SubjectContent>("Subjects");
		//public IMongoCollection<StudentProgress> StudentProgress => _database.GetCollection<StudentProgress>("StudentProgress");
		public static void ConfigureMappings()
		{
			BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

			if (!BsonClassMap.IsClassMapRegistered(typeof(SubjectContent)))
			{
				BsonClassMap.RegisterClassMap<SubjectContent>(cm =>
				{
					cm.AutoMap();
					cm.MapIdMember(s => s.Id);
				});
			}

			if (!BsonClassMap.IsClassMapRegistered(typeof(LessonContent)))
			{
				BsonClassMap.RegisterClassMap<LessonContent>(cm =>
				{
					cm.AutoMap();
					cm.MapIdMember(l => l.Id);
				});
			}

			if (!BsonClassMap.IsClassMapRegistered(typeof(Exercise)))
			{
				BsonClassMap.RegisterClassMap<Exercise>(cm =>
				{
					cm.AutoMap();
					cm.MapIdMember(e => e.Id);
				});
			}

			if (!BsonClassMap.IsClassMapRegistered(typeof(Question)))
			{
				BsonClassMap.RegisterClassMap<Question>(cm =>
				{
					cm.AutoMap();
					cm.MapIdMember(q => q.Qid);
				});
			}

			if (!BsonClassMap.IsClassMapRegistered(typeof(Answer)))
			{
				BsonClassMap.RegisterClassMap<Answer>(cm =>
				{
					cm.AutoMap();
					cm.MapIdMember(a => a.Id);
				});
			}

			if (!BsonClassMap.IsClassMapRegistered(typeof(Video)))
			{
				BsonClassMap.RegisterClassMap<Video>(cm =>
				{
					cm.AutoMap();
					cm.MapIdMember(v => v.Id);
				});
			}
		}
	// ... add others here ...
	}
}
