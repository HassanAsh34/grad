using System.Collections.Generic;
using MongoDB.Driver;

namespace grad.Application.exercise.DTOs
{
	public class QuestionDTO
	{
		public string question_type { get; set; }

		public string ?prompt_text { get; set; }

		public IFormFile ?prompt_image { get; set; }

		//public AnswerDTO CorrectAnswer { get; set; }

		public IEnumerable<AnswerDTO> Answers { get; set; }

		public int score { get; set; } = 10;
	}
}
