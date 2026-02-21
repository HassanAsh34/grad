using MongoDB.Bson.Serialization.Attributes;

namespace grad.Model
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
