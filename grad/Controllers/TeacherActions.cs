using grad.DTO;
using grad.Interfaces;
using grad.Services;
using Grpc.Core;
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
		//private readonly ISubjectServices _SubjectServices;
		private readonly ITokenServices _tokenServices;
		private readonly ITeacherServices _teacherServices;

		public TeacherActions(ISubjectServices subjectServices, ITokenServices tokenServices, ITeacherServices teacherServices)
		{
			//_SubjectServices = subjectServices ?? throw new ArgumentNullException(nameof(subjectServices));
			_tokenServices = tokenServices ?? throw new ArgumentNullException(nameof(tokenServices));
			_teacherServices = teacherServices ?? throw new ArgumentNullException(nameof(teacherServices));
		}


		//private readonly;
		[HttpGet("get-students")]
		public async Task<IActionResult> getStudents(CancellationToken cancellation)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			string sid = User.FindFirst("SubjectID")?.Value;
			if (Guid.TryParse(sid,out Guid Id))
			{
				ResultDTO result = await _teacherServices.ShowStudents(Id, cancellation);
				return StatusCode(result.StatusCode, new { message = result.Message, result = result.result });
			}
			else
				return StatusCode(StatusCodes.Status400BadRequest, new { message = "You still have been verified yet" });
			
		}

		

		[HttpGet("get-lessons")]
		public async Task<IActionResult> viewLessons(CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			string sid = User.FindFirst("SubjectID")?.Value;
			if (Guid.TryParse(sid, out Guid Id))
			{
				ResultDTO res = await _teacherServices.ViewLessons(Id, cancellationToken);
				return StatusCode(res.StatusCode, new { res.Message, res.result });
			}
			else
			{
				return StatusCode(StatusCodes.Status400BadRequest, new { message = "You still have been verified yet" });
			}
		}

		[HttpPost("Add-lesson")]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> addlesson([FromForm] LessonDTO lessonDTO, CancellationToken cancellationToken)
		{
			//we might add a further role for the teacher to show who has the ability to add lessons to the website
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			string sid = User.FindFirst("SubjectID")?.Value;
			if (Guid.TryParse(sid, out Guid Id))
			{
				lessonDTO.subjectID = Id;
				ResultDTO res = await _teacherServices.AddLesson(lessonDTO, cancellationToken);
				return StatusCode(res.StatusCode, new { res.Message, res.result });
			}	
			else
				return StatusCode(StatusCodes.Status400BadRequest, new { message = "You still have not been verified yet" });
		}
	}
}
