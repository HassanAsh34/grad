using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grad.Application.SubjectFeatures.DTOs
{
	internal class SubjectItemDTO
	{
		public Guid Id { get; set; }
		public string SubjectName { get; set; }

		public int LessonsCount { get; set; } = 0;

		public bool deaf_mute { get; set; } = false;
	}
}
