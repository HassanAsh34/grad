using Grad.Domain.Enums;

namespace Grad.Application.LessonFeatures.DTOs
{
	public class AddLessonDTO
	{
		public Guid SubjectId { get; set; }

		public Guid? UId { get; set; }

		public PerquisiteType PerquisiteType { get; set; }

		public Guid? Perquisite { get; set; }

		public string Title { get; set; }
	}
}
