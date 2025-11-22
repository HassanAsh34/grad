namespace grad.Model
{
	public class ModelExam
	{
		public string Id { get; private set; } = Guid.NewGuid().ToString();

		public string Title { get; set; }

		public int TotalQuestions { get; set; }



		public int PassingGrade { get; set; }

		//public IEnumerable<Exercise> Exercises { get; set; }
		public difficulty levelDifficulty { get; set; }
		public enum difficulty
		{
			easy,
			medium,
			hard
		}
	}
}
