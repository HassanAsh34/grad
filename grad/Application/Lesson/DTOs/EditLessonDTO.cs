namespace grad.Application.lesson.DTOs
{
	public class EditLessonDTO
	{
		public Guid SubjectId { get; set; }

		public Guid? UId { get; set; }

		public Guid Lid { get; set; }

		public string Title { get; set; }
	}
}
