using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grad.Application.SubmissionFeatures.DTOs;

namespace Grad.Application.ParentFeatures.DTOs
{
	public class SubjectStatsDTO
	{
		public List<SubmissionDTO> submissionDTOs { get; set; }

		public decimal avgGrades => submissionDTOs.Select(s => s.Percentage).Average();

		public int avgAttemptsUsed => (int)Math.Round(submissionDTOs.Select(s => s.AttemptsUsed).Average());
	}
}
