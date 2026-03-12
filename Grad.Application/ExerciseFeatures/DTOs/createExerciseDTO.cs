using System.ComponentModel.DataAnnotations;
using Grad.Domain.Enums;

namespace Grad.Application.ExerciseFeatures.DTOs
{
	public class CreateExerciseDTO
	{
		public Guid ?Lid { get; set; }

		public Guid ?Tid { get; set; }

		public Guid Sid { get; set; }


		[Required(ErrorMessage = "Title field is required")]
		[MinLength(3, ErrorMessage = "the Title must be at least 3 characters long")]
		public string Name { get; set; }

		public List<QuestionDTO> questions { get; set; } 

		public int total_score => questions?.Sum(q => q.score) ?? 0;

		public int PassingGradePercentage { get; set; }

		public Difficulty levelDifficulty { get; set; }
	}
}
