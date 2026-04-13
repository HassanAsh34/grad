using Grad.Domain.Enums;
namespace Grad.Domain.Model
{
	public class Exercise
	{
		
		public Guid Id { get; set; } = Guid.NewGuid();

		public string Name { get; set; }

		//public int total_questions { get; set; }

		public ExerciseType Type { get; set; }

		public List<Question> questions { get; set; } = new();

		//public int total_score => questions?.Sum(q => q.score) ?? 0;

	}
}
