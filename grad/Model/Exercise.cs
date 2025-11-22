namespace grad.Model
{
	public class Exercise
	{
		public string Id { get; private set; } = Guid.NewGuid().ToString();

		public string Title { get; set; }

		public IEnumerable<Question> Questions { get; set; }
	}
}
