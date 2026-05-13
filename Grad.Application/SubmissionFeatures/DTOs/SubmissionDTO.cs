using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grad.Domain.Enums;

namespace Grad.Application.SubmissionFeatures.DTOs
{
	public class SubmissionDTO
	{
		public string SubjectName { get; set; }

		public string LevelName { get; set; } = string.Empty;

		public Guid SubjectFK { get; set; }

		public Guid LevelFK { get; set; }

		public Guid? LessonID { get; set; }

		public decimal Percentage { get; set; }

		public decimal HighestPercentage { get; set; }

		public int AttemptsUsed { get; set; }

		public int RetakesRemaining { get; set; }

		public int TimeTakenInMinutes { get; set; }

		public DateTime SubmittedAt { get; set; }

	}
}
