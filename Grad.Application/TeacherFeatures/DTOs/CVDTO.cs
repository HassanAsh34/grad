using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Grad.Application.TeacherFeatures.DTOs
{
	public class CVDTO
	{
		public Guid Tid { get; set; }

		public IFormFile ?CV { get; set; }
	}
}
