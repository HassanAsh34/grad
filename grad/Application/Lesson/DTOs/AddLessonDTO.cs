namespace Grad_Structured.Application.lesson.DTOs
{
	public class AddLessonDTO
	{
		public Guid SubjectId { get; set; }

		public Guid? UId { get; set; }

		public string Title { get; set; }
	}
}
