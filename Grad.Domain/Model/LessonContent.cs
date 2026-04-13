using Grad.Domain.Enums;

namespace Grad.Domain.Model
{
	public class LessonContent
	{
		
		public Guid Id { get; private set; } = Guid.NewGuid();

		public string Title { get; set; }

		public Guid? Perquisite { get; set; }

		public Guid? Next { get; set; }

		public PerquisiteType NextType { get; set; } = PerquisiteType.None;

		public PerquisiteType PerquisiteType { get; set; }

		public List<Video> Videos { get; set; } = new();

		public Level? Level { get; set; }

	}
}
