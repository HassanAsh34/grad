using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace grad.Domain.Model
{
	public class SubjectContent
	{
		[BsonId]
		public Guid Id { get; set; }

		public Vocabulary ?Dictionary { get; set; }

		//public List<Exercise>? Levels { get; set; } = new();

		public List<LessonContent> Lessons { get; set; } = new();//SHORTENED WAY FOR new List<Lesson>()

		public List<Exercise> Quizzes { get; set; } = new();

	}
}
