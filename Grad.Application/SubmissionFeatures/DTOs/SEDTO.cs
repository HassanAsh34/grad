using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grad.Application.SubmissionFeatures.DTOs
{
	public class SEDTO
	{
		public Guid Eid { get; set; }

		public List<SADTO> SADTO { get; set; } = new List<SADTO>();
	}
}
