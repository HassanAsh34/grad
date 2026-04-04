using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grad.Domain.Model;

namespace Grad.Application.SubmissionFeatures.DTOs
{
	public class CreateSubmissionDTO
	{
		public Guid SubmittedBy { get; set; }

		public Guid SubjectFK { get; set; }

		public Guid LevelFK { get; set; }

		public Guid? LessonID { get; set; }

		public List<SEDTO> sEDTOs { get; set; } = new List<SEDTO>();
	}
}
