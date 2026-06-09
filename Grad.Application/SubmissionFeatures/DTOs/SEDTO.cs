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

		public decimal Score { get; set; } = 0; //Ai score for the exercise

		public List<SADTO> SADTO { get; set; } = new List<SADTO>();
	}
}
