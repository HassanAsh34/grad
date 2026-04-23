using Grad.Domain.Enums;

namespace Grad.Application.LessonFeatures.DTOs
{
	public class EditLessonDTO
	{
		public Guid SubjectId { get; set; }

		public Guid? UId { get; set; }

		public Guid Lid { get; set; }

		public Guid? Perquisite { get; set; }

		public PerquisiteType PerquisiteType { get; set; } = PerquisiteType.None;

		public string Title { get; set; }
		public string? Description { get; set; }
	}
}
