using System.ComponentModel.DataAnnotations;

namespace Grad.Application.SubjectFeatures.DTOs
{
	public class SubjectDTO
	{
		public Guid? SubjectId { get; set; }

		[Required]
		[RegularExpression("^[A-Za-z0-9ء-ي]+(?: ?[A-Za-z0-9ء-ي]+)*$", ErrorMessage = "Invalid Subject Name")]
		public string SubjectName { get; set; }

		public int? studentsCount { get; set; } = 0;

		public int? teachersCount { get; set; } = 0;

		public bool deaf_mute { get; set; }

		public bool AI_supported { get; set; } = false;

		public int? lessonsCount { get; set; } = 0;

		public int? levelsCount { get; set; } = 0;

		public int? WordCount { get; set; } = 0;

		public float? progress { get; set; } = 0;

		public decimal? AvgerageGrades { get; set; } = 0;

		public decimal? failure_rate { get; set; } = 0;

		public decimal? success_rate { get; set; } = 0;

		public int? submissionsCount { get; set; } = 0;

	}
}
