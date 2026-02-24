using Grad_Structured.Domain.Enums;

namespace Grad_Structured.Domain.Model
{
	public class Exercise
	{
		public Guid Id { get; private set; } = Guid.NewGuid();

		public string Name { get; set; }

		public int total_questions { get; set; } = 0;

		public List<Question> questions { get; set; } 

		public int total_score => questions.Sum(q=>q.score);

		public int PassingGrade { get; set; }

		public Difficulty levelDifficulty { get; set; }
	}
}
