using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace grad.Model
{
	public class Lesson
	{
		[BsonId]
		public string Id { get; private set; } 

		public string Title { get; set; }

		public string Description { get; set; }

		public string VideoPath { get; set; }

		public DateTime ReleaseDate { get; private set; } = DateTime.UtcNow.Date;

		//here we will add videos for the kids 

		//public IEnumerable<Level> levels { get; set; }  
	}
}
