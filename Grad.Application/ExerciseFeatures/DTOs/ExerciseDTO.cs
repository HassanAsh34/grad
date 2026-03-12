using Grad.Domain.Enums;


namespace Grad.Application.ExerciseFeatures.DTOs
{
	public class ExerciseDTO
	{
		public Guid Id { get; set; } = Guid.NewGuid();

		public string Name { get; set; }

		public int total_questions { get; set; } = 0;

		public IEnumerable<QuestionDTO> questions { get; set; }

		public int total_score => questions?.Sum(q => q.score) ?? 0;

		public int PassingGrade { get; set; }

		public Difficulty levelDifficulty { get; set; }
	}
}
