using grad.DTO;
using grad.Interfaces;
using grad.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace grad.Controllers
{
	[ApiController]
	[Route("/Student")]
	[Authorize(Roles = "Student")]
	public class StudentActions : ControllerBase
	{
		private readonly ITokenServices _tokenServices;
		private readonly IStudentServices _studentServices;

		public StudentActions(IStudentServices studentServices, ITokenServices tokenServices)
		{
			_tokenServices = tokenServices ?? throw new ArgumentNullException(nameof(tokenServices));
			_studentServices = studentServices ?? throw new ArgumentNullException(nameof(studentServices));
		}

		[HttpGet("View-Subjects")]
		public async Task<IActionResult> viewSubjects(CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			string id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (Guid.TryParse(id, out Guid guid))
			{
				ResultDTO result = await _studentServices.ViewSubjects(guid,cancellationToken: cancellationToken);
				return StatusCode(result.StatusCode, new { message = result.Message, result = result.result });
			}
			else
			{
				return Unauthorized(new { Message = "Invalid User" });
			}
		}

		[HttpGet("View-Subject/{sid}")]
		public async Task<IActionResult> viewSubject(string sid, CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			if (Guid.TryParse(sid, out Guid guid))
			{
				ResultDTO result = await _studentServices.viewSubject(guid, cancellationToken);
				return StatusCode(result.StatusCode, new { message = result.Message, result = result.result });
			}
			else
			{
				return Unauthorized(new { Message = "Invalid User" });
			}
		}

		[HttpPost("Enroll-subject/{sid}")]
		public async Task<IActionResult> EnrollSubject(string sid, CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			string stdId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			Guid stdGuid = Guid.Empty;
			Guid sidGuid = Guid.Empty;
			if (!Guid.TryParse(stdId, out stdGuid))
			{
				return Unauthorized();
			}
			else if (!Guid.TryParse(sid, out sidGuid))
			{
				return BadRequest(new { Message = "Invalid Subject Id" });
			}
			else
			{
				EnrollSubjectDTO enrollSubject = new EnrollSubjectDTO
				{
					stdFK = stdGuid,
					subFK = sidGuid
				};
				ResultDTO result = await _studentServices.EnrollSubject(enrollSubject, cancellationToken);
				return StatusCode(result.StatusCode, new { Message = result.Message, result = result });
			}
		}

		[HttpGet("View-Enrolled-Subjects")]
		public async Task<IActionResult> viewEnrolledSubjects(CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			string id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (Guid.TryParse(id, out Guid guid))
			{
				ResultDTO result = await _studentServices.ViewSubjects(guid,true,cancellationToken);
				return StatusCode(result.StatusCode, new { message = result.Message, result = result.result });
			}
			else
			{
				return Unauthorized(new { Message = "Invalid User" });
			}
		}

		//view lesson

		[HttpGet("View-Lessons/{Sid}")]
		public async Task<IActionResult> viewLessons(string Sid,CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			string id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (Guid.TryParse(id, out Guid guid))
			{
				if(Guid.TryParse(Sid, out Guid Gsid))
				{
					EnrollSubjectDTO enroll = new EnrollSubjectDTO
					{
						stdFK = guid,
						subFK = Gsid
					};
					ResultDTO result = await _studentServices.viewLessons(enroll, cancellationToken);
					return StatusCode(result.StatusCode, new { message = result.Message, result = result.result });
				}
				else
				{
					return BadRequest("Invalid Subject");
				}
			}
			else
			{
				return Unauthorized(new { Message = "Invalid User" });
			}
		}

		//view lesson
		[HttpGet("View-lesson")]
		public async Task<IActionResult> viewLesson([FromBody]GetLessonDTO getLesson, CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			string id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (Guid.TryParse(id, out Guid guid))
			{
				if(Guid.TryParse(getLesson.Sid, out Guid Gsid) && Guid.TryParse(getLesson.Lid, out Guid Glid))
				{
					LessonContentDTO lessonDTO = new LessonContentDTO
					{
						Id = Glid,
						SubjectId = Gsid
					};
					ResultDTO result = await _studentServices.viewLesson(lessonDTO, cancellationToken);
					//if(result.StatusCode == 200 && result.result is VideoDTO lesson)
					//{
					//	//lesson.videoUrl = $"{Request.Scheme}://{Request.Host}/{lesson.VideoPath}";
					//	//Console.WriteLine(lesson.videoUrl);
					//}
					return StatusCode(result.StatusCode, new { message = result.Message, result = result.result });
				}
				else
				{
					return BadRequest("Invalid Subject or Lesson");
				}
			}
			else
			{
				return Unauthorized();
			}
		}
	}
}
