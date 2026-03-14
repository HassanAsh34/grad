using Microsoft.AspNetCore.Http;

namespace Grad.Application.ExerciseFeatures.DTOs
{
	public class QuestionDTO
	{
		public Guid Qid { get; set; }
		public string question_type { get; set; }

		public string ?prompt_text { get; set; }

		public IFormFile? prompt_image { get; set; }

		//public AnswerDTO CorrectAnswer { get; set; }

		public IEnumerable<AnswerDTO> Answers { get; set; } 

		public int score { get; set; } = 10;
	}
}
