using grad.DTO;
using grad.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace grad.Controllers
{
	[ApiController]
	[Route("/Teacher")]
	[Authorize(Roles = "Teacher")]
	public class TeacherActions :ControllerBase
	{
		private readonly ISubjectServices _SubjectServices;

		public TeacherActions(ISubjectServices subjectServices)
		{
			_SubjectServices = subjectServices ?? throw new ArgumentNullException(nameof(subjectServices));
		}


		//private readonly;
		[HttpGet("get-students")]
		public async Task<IActionResult> getStudents()
		{
			throw new NotImplementedException();
		}

		

		[HttpGet("get-lessons")]
		public async Task<IActionResult> viewLessons(CancellationToken cancellationToken)
		{
			string sid = User.FindFirst("SubjectID")?.Value;
			if (sid.IsNullOrEmpty())
				return StatusCode(StatusCodes.Status400BadRequest, new { message = "You still have been verified yet" });
			else
			{
				LessonDTO lessonDTO = new LessonDTO
				{
					subjectID = sid
				};
				ResultDTO res = await _SubjectServices.ViewLessons(lessonDTO, cancellationToken);
				return StatusCode(res.StatusCode, new { res.Message, res.result });
			}
		}

		[HttpPost("Add-lesson")]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> addlesson([FromForm] LessonDTO lessonDTO, CancellationToken cancellationToken)
		{
			//we might add a further role for the teacher to show who has the ability to add lessons to the website
			string sid = User.FindFirst("SubjectID")?.Value;
			if (sid.IsNullOrEmpty())
				return StatusCode(StatusCodes.Status400BadRequest, new { message = "You still have not been verified yet" });
			else
			{
				lessonDTO.subjectID = sid;
				ResultDTO res = await _SubjectServices.AddLesson(lessonDTO, cancellationToken);
				return StatusCode(res.StatusCode, new { res.Message, res.result });
			}
		}


		
	}
}
