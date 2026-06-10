using Grad.Domain.Enums;


namespace Grad.Application.ExerciseFeatures.DTOs
{
	public class ExerciseDTO
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public ExerciseType Type { get; set; }
		public string Name { get; set; }

		public Dictionary<string, int> ? AI_letters { get; set; }

		public int Round { get; set; } = 0;

		public IEnumerable<QuestionDTO>? questions { get; set; }

		public int total_questions  => Type == ExerciseType.AI || Type == ExerciseType.AI_Word ? AI_letters?.Count ?? 0 : questions?.Count()  ?? 0;

		public IEnumerable<AnswerDTO> ?answers { get; set; }

		public int total_score => Type == ExerciseType.AI ? AI_letters?.Values.Sum() * 10 ?? 0 : questions?.Sum(q => q.score) ?? 0;
		//public Difficulty levelDifficulty { get; set; }
	}
}
