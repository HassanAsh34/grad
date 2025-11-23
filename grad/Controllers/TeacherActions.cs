using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace grad.Controllers
{
	[ApiController]
	[Route("/Teacher")]
	public class TeacherActions :ControllerBase
	{
		//private readonly;
		public TeacherActions()
		{
		}
		[HttpPost("get-students")]
		[Authorize(Roles = "Teacher")]
		public async Task<IActionResult> getStudents()
		{
			throw new NotImplementedException();
		}

		//[HttpPost("")]
	}
}
