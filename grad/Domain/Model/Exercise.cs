using grad.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace grad.Domain.Model
{
	public class Exercise
	{
		[BsonId]
		public Guid Id { get; set; } = Guid.NewGuid();

		public string Name { get; set; }

		public int total_questions { get; set; } = 0;

		public List<Question> questions { get; set; } = new List<Question>();

		public int total_score => questions.Sum(q=>q.score);

		public int PassingGrade { get; set; }

		public Difficulty levelDifficulty { get; set; }
	}
}
