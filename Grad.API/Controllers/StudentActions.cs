using Azure.Core;
using Grad.Application.Common.DTOs;
using Grad.Application.Common.Interfaces;
using Grad.Application.ExerciseFeatures.DTOs;
using Grad.Application.LessonFeatures.DTOs;
using Grad.Application.StudentFeatures.DTOs;
using Grad.Application.StudentFeatures.Interfaces;
using Grad.Application.SubmissionFeatures.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Grad.API.Controllers
{
	[ApiController]
	[Route("/Student")]
	[Authorize(Roles = "Student")]
	public class StudentActions : ControllerBase
	{
		private readonly ITokenServices _tokenServices;
		private readonly IStudentServices _studentServices;
		private readonly ILogger<StudentActions> _logger;

		public StudentActions(IStudentServices studentServices, ITokenServices tokenServices, ILogger<StudentActions> logger)
		{
			_tokenServices = tokenServices ?? throw new ArgumentNullException(nameof(tokenServices));
			_studentServices = studentServices ?? throw new ArgumentNullException(nameof(studentServices));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
			//string role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value; leave for now
			if (Guid.TryParse(id, out Guid guid))
			{
				ResultDTO result = await _studentServices.ViewSubjects(guid,cancellationToken: cancellationToken);
				return StatusCode(result.StatusCode, new { Message = result.Message, Data = result.result });
			}
			else
			{
				return Unauthorized();
			}
		}

		
		[HttpGet("View-Submissions")]
		public async Task<IActionResult> viewSubmissions(CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			string id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (Guid.TryParse(id, out Guid guid))
			{
				ResultDTO result = await _studentServices.viewSubmissions(guid, cancellationToken);
				return StatusCode(result.StatusCode, new { Message = result.Message, Data = result.result });
			}
			else
			{
				return Unauthorized();
			}
		}

		//[HttpGet("View-Subject/{sid}")]
		//public async Task<IActionResult> viewSubject(string sid, CancellationToken cancellationToken)
		//{
		//	string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
		//	string uid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
		//	if (!await _tokenServices.IsTokenBlacklisted(accessToken))
		//	{
		//		return Unauthorized();
		//	}
		//	if (Guid.TryParse(sid, out Guid gsid) && Guid.TryParse(uid, out Guid guid))
		//	{
		//		ResultDTO result = await _studentServices.view
		//		return StatusCode(result.StatusCode, new { message = result.Message, result = result.result });
		//	}
		//	else
		//	{
		//		return Unauthorized();
		//	}
		//}

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
				return StatusCode(result.StatusCode, new { Message = result.Message, Data = result });
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
				return StatusCode(result.StatusCode, new { Message = result.Message, Data = result.result });
			}
			else
			{
				return Unauthorized();
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
					return StatusCode(result.StatusCode, new { Message = result.Message, Data = result.result });
				}
				else
				{
					return BadRequest("Invalid Subject");
				}
			}
			else
			{
				return Unauthorized();
			}
		}

		//view lesson
		[HttpGet("View-lesson/{sid}/{lid}")]
		public async Task<IActionResult> viewLesson(string sid,string lid,CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			string id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (Guid.TryParse(id, out Guid guid))
			{
				if(Guid.TryParse(sid, out Guid Gsid) && Guid.TryParse(lid, out Guid Glid))
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
					return StatusCode(result.StatusCode, new { Message = result.Message, Data = result.result });
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

		[HttpGet("View-Exercise")]
		public async Task<IActionResult> viewExercise([FromHeader] Guid sid, [FromHeader] Guid lid, [FromHeader] Guid eid, CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			string id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (Guid.TryParse(id, out Guid guid))
			{
				LevelDTO level = new LevelDTO
				{
					ID = eid,
					Lid = lid,
					Sid = sid
				};
				ResultDTO result = await _studentServices.ViewExerciseQuize(level, guid, cancellationToken);
				return StatusCode(result.StatusCode, new { Message = result.Message, Data = result.result });
			}
			else
			{
				return Unauthorized();
			}
		}

		[HttpGet("Start-Quiz")]
		public async Task<IActionResult> viewQuiz([FromHeader] Guid sid, [FromHeader] Guid eid, CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			string id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (Guid.TryParse(id, out Guid guid))
			{
				LevelDTO level = new LevelDTO
				{
					ID = eid,
					Sid = sid
				};
				ResultDTO result = await _studentServices.ViewExerciseQuize(level, guid, cancellationToken);
				return StatusCode(result.StatusCode, new { Message = result.Message, Data = result.result });
			}
			else
			{
				return Unauthorized();
			}
		}

		[HttpPost("Complete-lesson")]
		public async Task<IActionResult> completeLesson([FromBody] LessonDTO getLesson, CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			string id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (Guid.TryParse(id, out Guid guid))
			{
				if (Guid.TryParse(getLesson.Sid, out Guid Gsid) && Guid.TryParse(getLesson.Lid, out Guid Glid))
				{
					CompletelessonDTO lessonDTO = new CompletelessonDTO
					{
						Lid = Glid,
						Sid = Gsid,
						uid	= guid
					};
					ResultDTO result = await _studentServices.completeLesson(lessonDTO, cancellationToken);
					//if(result.StatusCode == 200 && result.result is VideoDTO lesson)
					//{
					//	//lesson.videoUrl = $"{Request.Scheme}://{Request.Host}/{lesson.VideoPath}";
					//	//Console.WriteLine(lesson.videoUrl);
					//}
					return StatusCode(result.StatusCode, new { Message = result.Message, Data = result.result });
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

		[HttpPost("Submit-Answers")]
		public async Task<IActionResult> submitAnswers([FromBody] CreateSubmissionDTO submitAnswers, CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("AccessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			string id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (Guid.TryParse(id, out Guid guid))
			{
				submitAnswers.SubmittedBy = guid;
				ResultDTO result = await _studentServices.createSubmission(submitAnswers, cancellationToken);
				return StatusCode(result.StatusCode, new { Message = result.Message, Data = result.result });
			}
			else
				return Unauthorized();
		}
	}
}
