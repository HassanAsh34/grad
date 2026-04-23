using System.ComponentModel.DataAnnotations;
using Grad.Domain.Enums;

namespace Grad.Application.ExerciseFeatures.DTOs
{
	public class CreateLevelDTO
	{
		public Guid ?Lid { get; set; }

		public Guid ?Tid { get; set; }

		public Guid Sid { get; set; }


		[Required(ErrorMessage = "Title field is required")]
		[MinLength(3, ErrorMessage = "the Title must be at least 3 characters long")]
		public string Name { get; set; }

		public List<ExerciseDTO> ExerciseDTOs { get; set; } 

		public int total_score => ExerciseDTOs?.Sum(q => q.total_score) ?? 0;

		public int PassingGradePercentage { get; set; }

		public PerquisiteType PerquisiteType { get; set; } = PerquisiteType.None;

		public Guid ? PerquisiteID { get; set; }

		public Difficulty levelDifficulty { get; set; }
	}
}
