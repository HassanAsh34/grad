namespace Grad.Application.LessonFeatures.DTOs
{
	public class DeleteLessonDTO
	{
		public Guid SubjectId { get; set; }

		public Guid? UId { get; set; }

		public Guid Lid { get; set; }
	}
}
