using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grad.Domain.Enums;

namespace Grad.Application.ExerciseFeatures.DTOs
{
	public class EditLevelDTO
	{
		public Guid Id { get; set; }
		public Guid? Lid { get; set; }

		public Guid? Tid { get; set; }

		public Guid Sid { get; set; }


		//[Required(ErrorMessage = "Title field is required")]
		[MinLength(3, ErrorMessage = "the Title must be at least 3 characters long")]
		public string ?Name { get; set; }

		public List<ExerciseDTO> ExerciseDTOs { get; set; }

		public int total_score => ExerciseDTOs?.Sum(q => q.total_score) ?? 0;

		public int PassingPercentage { get; set; } = -1;

		public Difficulty ?levelDifficulty { get; set; }
	}
}


