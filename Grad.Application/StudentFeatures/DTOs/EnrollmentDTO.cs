using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grad.Application.StudentFeatures.DTOs
{
	public class EnrollmentDTO
	{
		public Guid SubjectId { get; set; }

		public float Progress { get; set; } = 0;
	}
}
