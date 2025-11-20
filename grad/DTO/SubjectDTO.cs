using System.ComponentModel.DataAnnotations;

namespace grad.DTO
{
	public class SubjectDTO
	{
		public string? SubjectId { get; set; }

		[Required]
		[RegularExpression("^[A-Za-z]{4,}$")]
		public string SubjectName { get; set; }

		public int? studentsCount { get; set; } = 0;

		public int? teachersCount { get; set; } = 0;

		
	}
}
