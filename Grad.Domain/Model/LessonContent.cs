namespace Grad.Domain.Model
{
	public class LessonContent
	{
		
		public Guid Id { get; private set; } = Guid.NewGuid();

		public string Title { get; set; }

		public List<Video> Videos { get; set; } = new();

		public Level? Level { get; set; }

	}
}
