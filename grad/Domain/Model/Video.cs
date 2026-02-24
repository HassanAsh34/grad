using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace Grad_Structured.Domain.Model
{
	public class Video
	{
		[BsonId]
		public Guid Id { get; private set; } = Guid.NewGuid();

		public string Title { get; set; }

		public string Description { get; set; }

		public string VideoPath { get; set; }

		public string Uploaded_by { get; set; }

		public DateTime ReleaseDate { get; private set; } = DateTime.UtcNow.Date;

		//here we will add videos for the kids 

		//public IEnumerable<Level> levels { get; set; }  
	}
}
