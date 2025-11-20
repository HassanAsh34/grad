namespace grad.Model
{
	public class Qusetion
	{
		public string question { get; set; }

		//public  image 

		public Answer CorrectAnswer { get; set; }

		public IEnumerable<Answer> Answers { get; set; }

	}
}
