using Microsoft.AspNetCore.Http;

namespace Grad.Application.ExerciseFeatures.DTOs
{
	public class AnswerDTO
	{
		public Guid Id { get; set; }
		public string ?answer { get; set; }

		public string ?imgPath { get; set; }

		public IFormFile? IMG { get; set; }

		public bool isCorrect { get; set; }

		//public bool isCorrect { get; set; }
	}
}
