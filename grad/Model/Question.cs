namespace grad.Model
{
	public class Question
	{
		public string question { get; set; }

		public string IMG { get; set; }

		public Answer CorrectAnswer { get; set; }

		public IEnumerable<Answer> Answers { get; set; }

	}
}
