using Grad.Domain.Enums;


namespace Grad.Application.ExerciseFeatures.DTOs
{
	public class ExerciseDTO
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public ExerciseType Type { get; set; }
		public string Name { get; set; }
		public IEnumerable<QuestionDTO> questions { get; set; }


		public int total_questions  =>  questions?.Count() ?? 0;

		public IEnumerable<AnswerDTO> ?answers { get; set; }

		public int total_score => questions?.Sum(q => q.score) ?? 0;
		public Difficulty levelDifficulty { get; set; }
	}
}
