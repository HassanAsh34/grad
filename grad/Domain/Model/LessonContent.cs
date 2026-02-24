using MongoDB.Bson.Serialization.Attributes;

namespace Grad_Structured.Domain.Model
{
	public class LessonContent
	{
		[BsonId]
		public Guid Id { get; private set; } = Guid.NewGuid();

		public string Title { get; set; }

		public List<Video> Videos { get; set; } = new();

		public Exercise ?Exercises { get; set; } 

	}
}
