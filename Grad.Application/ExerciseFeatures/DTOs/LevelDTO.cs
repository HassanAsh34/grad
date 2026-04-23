using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grad.Domain.Enums;

namespace Grad.Application.ExerciseFeatures.DTOs
{
	public class LevelDTO
	{
		public Guid ID { get; set; }

		public Guid Sid { get; set; }

		public Guid Lid { get; set; }

		public int DurationInMinutes { get; set; } = -1;

		public int AttemptsAllowed { get; set; } = 0;	



		public string Name { get; set; }
		public IEnumerable<ExerciseDTO> Exercise { get; set; }
		public int PassingPercentage { get; set; }
		public Difficulty levelDifficulty { get; set; }
		public int total_score => Exercise?.Sum(e => e.total_score) ?? 0;

		public int PassingScore => total_score * PassingPercentage / 100;
	}
}
