using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grad.Domain.Enums
{
	public enum InqueryType
	{
		Complaint = 1, // admin submitted by student or parent or teacher
		Suggestion = 2, // admin submitted by student or parent or teacher
		Question = 3, //submitted by student 
		Answer = 4, // teacher or admin
		TechnicalSupport = 5, // admin submitted by student or parent or teacher
	}
}
