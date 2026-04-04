namespace Grad.Domain.Model
{
	public class SubjectContent
	{
		
		public Guid Id { get; set; }

		public Vocabulary ?Dictionary { get; set; }

		//public List<Exercise>? Levels { get; set; } = new();

		public List<LessonContent> Lessons { get; set; } = new();//SHORTENED WAY FOR new List<Lesson>()

		public List<Level> Quizzes { get; set; } = new();

	}
}
