using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grad.Domain.Model
{
	public class Submission
	{
		public Guid Id { get; private set; } = Guid.NewGuid();

		public DateTime SubmittedAt { get; private set; } = DateTime.UtcNow;

		public Guid SubmittedBy { get; set; }

		public Guid ?SubjectFK { get; set; }

		public Guid LevelFK { get; set; }

		public Guid ?LessonID { get; set; }

		public decimal Percentage { get; set; } = 0;

		public bool Passed { get; set; } = false;

		public Subject ?Subject { get; set; }

		public Student Student { get; set; }

	}
}
