namespace grad.Model
{
	public class Level
	{
		public string Id { get; private set; } = Guid.NewGuid().ToString();

		public string Name { get; set; }

		public IEnumerable<ModelExam> models { get; set; }

		

	}
}
