using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace grad.Model
{
	public class SubjectContent
	{
		[BsonId]
		public string Id { get; set; }

		public Vocabulary ?Dictionary { get; set; }

		public List<Level>? Levels { get; set; } = new();

		public List<Lesson> Lessons { get; set; } = new(); //SHORTENED WAY FOR new List<Lesson>()

	}
}
