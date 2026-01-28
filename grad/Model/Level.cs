namespace grad.Model
{
	public class Level
	{
		public Guid Id { get; private set; } = Guid.NewGuid();

		public string Name { get; set; }

		public int total_questions { get; set; } = 0;


		public List<Question> questions { get; set; } 

		public int total_score => questions.Sum(q=>q.score); 
	}
}
