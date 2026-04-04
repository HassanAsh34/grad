using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grad.Application.SubmissionFeatures.DTOs
{
	internal class Grade
	{
		public decimal Percentage { get; set; } = 0;

		public bool Passed { get; set; } = false;
	}
}
