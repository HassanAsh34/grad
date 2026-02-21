namespace grad.DTO
{
	public class AddLessonDTO
	{
		public Guid SubjectId { get; set; }

		public Guid? UId { get; set; }

		public string Title { get; set; }
	}
}
