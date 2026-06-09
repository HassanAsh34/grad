using Grad.Domain.Enums;
namespace Grad.Domain.Model
{
	public class Exercise
	{
		
		public Guid Id { get; set; } = Guid.NewGuid();

		public string Name { get; set; }

		//public int total_questions { get; set; }

		public ExerciseType Type { get; set; }

		public Dictionary<string,int> AI_letters { get; set; } = new();

		public List<Question> questions { get; set; } = new();

		public int total_score => Type == ExerciseType.AI ? AI_letters?.Values.Sum() * 10 ?? 0 : questions?.Sum(q => q.score) ?? 0;

	}
}
