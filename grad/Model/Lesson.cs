namespace grad.Model
{
	public class Lesson
	{
		public string Id { get; private set; } = Guid.NewGuid().ToString();

		public string Title { get; set; }

		public string Description { get; set; }

		//public IEnumerable<Level> levels { get; set; }  
	}
}
