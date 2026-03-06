namespace grad.Application.exercise.DTOs
{
	public class AnswerDTO
	{
		public string ?answer { get; set; }

		public IFormFile ?IMG { get; set; }

		public bool isCorrect { get; set; }

		//public bool isCorrect { get; set; }
	}
}
