using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Grad.Application.TeacherFeatures.DTOs
{
	public class UploadCVDTO
	{
		public IFormFile file { get; set; }	
	}
}
