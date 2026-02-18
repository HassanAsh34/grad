using System.Text.Json;
using System.Threading;
using Azure.Core;
using grad.DTO;
using grad.Interfaces;
using grad.Model;
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
	public class TeacherActions : ControllerBase
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
			if (Guid.TryParse(sid, out Guid Id))
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
				if(ModelState.IsValid == false)
				{
					return BadRequest(new {ModelState});
				}
				ResultDTO res = await _teacherServices.AddLesson(lessonDTO, cancellationToken);
				return StatusCode(res.StatusCode, new { res.Message, res.result });
			}
			else
				return StatusCode(StatusCodes.Status400BadRequest, new { message = "You still have not been verified yet" });
		}

		[HttpGet("Get-Student/{sid}")]
		public async Task<IActionResult> getStudent(string sid, CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			if (Guid.TryParse(sid, out Guid Id))
			{
				ProfileDTO profile = new ProfileDTO
				{
					Id = Id,
					Role = "Student"
				};
				ResultDTO res = await _teacherServices.ViewStudent(profile, cancellationToken);
				return StatusCode(res.StatusCode, new { res.Message, res.result });
			}
			else
				return BadRequest(new { message = "invalid student id" });
		}


		[HttpGet("Home-Screen")]
		public async Task<IActionResult> viewSubject(CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			string sid = User.FindFirst("SubjectID")?.Value;
			if (Guid.TryParse(sid, out Guid Id))
			{
				ResultDTO res = await _teacherServices.ViewSubject(Id, cancellationToken);
				return StatusCode(res.StatusCode, new { res.Message, res.result });
			}
			else
				return StatusCode(StatusCodes.Status400BadRequest, new { message = "You still have not been verified yet" });
		}

		[HttpPatch("Edit-Lesson/{lid}")]
		public async Task<IActionResult> editLisson(string lid,[FromBody] EditLessonDTO lessonDTO,CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			string sid = User.FindFirst("SubjectID")?.Value;
			if (Guid.TryParse(sid, out Guid Id))
			{
				lessonDTO.subjectID = Id;
				if (Guid.TryParse(lid, out Guid lguid))
				{
					lessonDTO.Id = lguid;
				}
				else
					return BadRequest(new { message = "invalid lesson id" });
				//if(ModelState.IsValid == false)
				//{
				//	return BadRequest(new {ModelState});
				//}
				ResultDTO res = await _teacherServices.EditLesson(lessonDTO, cancellationToken);
				return StatusCode(res.StatusCode, new { res.Message, res.result });
			}
			else
				return StatusCode(StatusCodes.Status400BadRequest, new { message = "You still have not been verified yet" });
		}

		[HttpDelete("Remove-Lesson/{lid}")]
		public async Task<IActionResult> removeLesson(string lid,CancellationToken cancellationToken)
		{
			string accesstoken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accesstoken))
			{
				return Unauthorized();
			}
			string sid = User.FindFirst("SubjectID")?.Value;
			if (Guid.TryParse(sid, out Guid Id))
			{
				Guid gsid = Id;
				Guid glid = Guid.Empty;
				if (Guid.TryParse(lid, out Guid lguid))
				{
					glid = lguid;
				}
				else
					return BadRequest(new { message = "invalid lesson id" });
				ResultDTO res = await _teacherServices.DeleteLesson(gsid,glid,cancellationToken);
				return StatusCode(res.StatusCode, new { res.Message, res.result });
			}
			else
				return StatusCode(StatusCodes.Status400BadRequest, new { message = "You still have not been verified yet" });
		}

		//public 

		[HttpPost("Add-words-to-Dictionary")]
		//[Consumes("multipart/form-data")]
		public async Task<IActionResult> addWordsToDictionary([FromForm] AddVocabDTO vocabDTO, CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value;
			if(!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			string sid = User.FindFirst("subjectID")?.Value;
			if (Guid.TryParse(sid, out Guid gsid))
			{
				if (vocabDTO.word == null || vocabDTO.files == null)
					return BadRequest("Invalid input");
				if (vocabDTO.word.Count != vocabDTO.files.Count)
					return BadRequest("Each word must have exactly one file");
				//return BadRequest(new { message = vocabDTO.word.Count - vocabDTO.files.Count  > 1 ? $"{vocabDTO.word.Count - vocabDTO.files.Count} words are messing images" : "One word is messing an image" });
				else
				{
					vocabDTO.sid = gsid;
					ResultDTO res = await _teacherServices.addWords(vocabDTO, cancellationToken);
					return StatusCode(res.StatusCode, new { res.Message});
				}
			}
			else
				return StatusCode(StatusCodes.Status400BadRequest, new { message = "You still have not been verified yet" });
		}
		
		//implement add exercises
			
	}
}
