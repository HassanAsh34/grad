using Microsoft.AspNetCore.Http;

namespace Grad.Application.ExerciseFeatures.DTOs
{
	public class QuestionDTO
	{
		public Guid Qid { get; set; }
		public string ?prompt_text { get; set; }

		public IFormFile? prompt_image { get; set; }
		public string? imgPath { get; set; }

		public AnswerDTO ?Answer { get; set; }

		public IEnumerable<AnswerDTO> ?Answers { get; set; } 

		public int score { get; set; } = 10;
	}
}
