namespace Grad.Domain.Model
{
	public class Question
	{
		public Guid Qid { get; set; } = Guid.NewGuid();

		public string ?prompt_text { get; set; }

		public string ?prompt_image { get; set; }

		public Guid ?CorrectAnswer { get; set; }

		public List<Answer> Answers { get; set; } = new();

		public Answer Answer { get; set; }

		public int score { get; set; } = 10;
	}
}
