using MongoDB.Bson.Serialization.Attributes;

namespace grad.Model
{
	public class StudentProgress
	{
		[BsonId]
		public Guid Id { get; set; }
		public List<string> CompletedLessonIds { get; set; } = new();
		public int ProgressPercent { get; set; }
	}
}