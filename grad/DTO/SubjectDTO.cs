using System.ComponentModel.DataAnnotations;

namespace grad.DTO
{
	public class SubjectDTO
	{
		public Guid? SubjectId { get; set; }

		[Required]
		[RegularExpression("^[A-Za-z0-9]+(?: ?[A-Za-z0-9]+)*$", ErrorMessage = "Invalid Subject Name")]
		public string SubjectName { get; set; }

		public int? studentsCount { get; set; } = 0;

		public int? teachersCount { get; set; } = 0;

		public bool deaf_mute { get; set; }

		public int? lessonsCount { get; set; } = 0;

		public int? levelsCount { get; set; } = 0;
	}
}
