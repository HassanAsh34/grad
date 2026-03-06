using grad.Application.Common.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using grad.Application.teacher.Interfaces;
using grad.Application.teacher.DTOs;
using grad.Application.Common.Interfaces;
using grad.Application.subject.Interfaces;
using grad.Application.lesson.DTOs;
using grad.Application.subject.DTOs;
using grad.Application.Lesson.DTOs;
using grad.Application.exercise.DTOs;
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
		[HttpGet("get-students/{sid}")]//done
		public async Task<IActionResult> getStudents(string sid,CancellationToken cancellation)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			string uid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			//string sid = User.FindFirst("SubjectID")?.Value;
			if (Guid.TryParse(uid, out Guid Uguid))
			{
				if (Guid.TryParse(sid, out Guid Id))
				{
					ResultDTO result = await _teacherServices.ShowStudents(new TeacherSubjectDTO()
					{
						SubjectId = Id,
						TeacherId = Uguid
					}, cancellation);
					return StatusCode(result.StatusCode, new { message = result.Message, result = result.result });
				}
				else
					return NotFound("Subject wasn't found");
			}
			else
				return Unauthorized();

		}



		[HttpGet("get-lessons/{sid}")]//done
		public async Task<IActionResult> viewLessons(string sid,CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			string uid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			//string sid = User.FindFirst("SubjectID")?.Value;
			if (Guid.TryParse(uid, out Guid Uguid))
			{
				if (Guid.TryParse(sid, out Guid Id))
				{
					ResultDTO res = await _teacherServices.ViewLessons(new TeacherSubjectDTO()
					{
						SubjectId = Id,
						TeacherId = Uguid
					}, cancellationToken);
					return StatusCode(res.StatusCode, new { message = res.Message, result = res.result });
				}
				else
					return NotFound("No lessons were found");
			}
			else
				return Unauthorized();

		}

		[HttpGet("View-Lesson/{Sid}/{Lid}")]//done
		public async Task<IActionResult> viewLesson(string Sid,string Lid, CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			//string Uid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			//string sid = User.FindFirst("SubjectID")?.Value;
			//if (Guid.TryParse(Uid, out Guid Uguid))
			//{
			if (Guid.TryParse(Sid, out Guid Id) && Guid.TryParse(Lid, out Guid lguid))
			{
				//if (Guid.TryParse(lid, out Guid lguid))
				//{
				ResultDTO res = await _teacherServices.ViewLesson(new LessonContentDTO { SubjectId = Id, Id = lguid }, cancellationToken);
				return StatusCode(res.StatusCode, new { res.Message, res.result });
			}
			else
				return NotFound("No lessons were found");
			//}
			//else
			//	return StatusCode(StatusCodes.Status400BadRequest, new { message = "You still have not been verified yet" });
		}


		[HttpPost("Add-lesson")]//done
		//[Consumes("multipart/form-data")]//need to be fixed
		public async Task<IActionResult> addlesson([FromForm] AddLessonDTO lessonDTO, CancellationToken cancellationToken)
		{
			//we might add a further role for the teacher to show who has the ability to add lessons to the website
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			string uid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			//string sid = User.FindFirst("SubjectID")?.Value;
			if (Guid.TryParse(uid, out Guid Id))
			{
				lessonDTO.UId = Id;
				if (ModelState.IsValid == false)
				{
					return BadRequest(new {ModelState});
				}
				ResultDTO res = await _teacherServices.AddLesson(lessonDTO, cancellationToken);
				return StatusCode(res.StatusCode, new { res.Message, res.result });
			}
			else
				return Unauthorized();
		}


		[HttpPost("Upload-Video")]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> uploadVideo([FromForm] VideoDTO videoDTO, CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			videoDTO.Uploaded_by = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			//if (Guid.TryParse(videoDTO.Sid, out Guid Id))
			//{
			//	videoDTO.SubjectId = Id;
			if (ModelState.IsValid == false)
			{
				return BadRequest(new { ModelState });
			}
			ResultDTO res = await _teacherServices.UploadVideo(videoDTO, cancellationToken);
			return StatusCode(res.StatusCode, new { res.Message, res.result });
		//}
			//else
			//	return StatusCode(StatusCodes.Status400BadRequest, new { message = "You still have not been verified yet" });
		}



		[HttpGet("Get-Student/{sid}")]//done
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
				return NotFound(new { message = "Student wasn't found" });
		}


		[HttpGet("view-subject/{sid}")]//done
		public async Task<IActionResult> viewSubject(string sid,CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			string Uid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			//string sid = User.FindFirst("SubjectID")?.Value;
			if (Guid.TryParse(Uid, out Guid Uguid))
			{
				if (Guid.TryParse(sid, out Guid Id))
				{	
					ResultDTO res = await _teacherServices.ViewSubject(new TeacherSubjectDTO { SubjectId = Id, TeacherId = Uguid }, cancellationToken);
					return StatusCode(res.StatusCode, new { res.Message, res.result });
				}
				else
					return StatusCode(StatusCodes.Status400BadRequest, new { message = "Invalid subject ID" });
			}
			else
				return Unauthorized();
		}

		[HttpGet("Home")]//done
		public async Task<IActionResult> viewSubjects(CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			string Uid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			//string sid = User.FindFirst("SubjectID")?.Value;
			if (Guid.TryParse(Uid, out Guid Id))
			{
				ResultDTO res = await _teacherServices.ViewSubjects(Id, cancellationToken);
				return StatusCode(res.StatusCode, new { res.Message, res.result });
			}
			else
				return Unauthorized();
		}

		[HttpPatch("Edit-Lesson")]
		public async Task<IActionResult> editLisson([FromBody] EditLessonDTO lessonDTO, CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			string uid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			if (Guid.TryParse(uid, out Guid Id))
			{
				lessonDTO.UId = Id;
				ResultDTO res = await _teacherServices.EditLesson(lessonDTO, cancellationToken);
				return StatusCode(res.StatusCode, new { res.Message, res.result });
			}
			else
				return Unauthorized();
		}

		[HttpDelete("Remove-Lesson")]
		public async Task<IActionResult> removeLesson([FromBody] DeleteLessonDTO lessonDTO, CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			string uid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			if (Guid.TryParse(uid, out Guid Id))
			{
				lessonDTO.UId = Id;
				ResultDTO res = await _teacherServices.DeleteLesson(lessonDTO, cancellationToken);
				return StatusCode(res.StatusCode, new { res.Message, res.result });
			}
			else
				return Unauthorized();
		}

		//[HttpDelete("Remove-Lesson/{lid}")]
		//public async Task<IActionResult> removeLesson(string lid, CancellationToken cancellationToken)
		//{
		//	string accesstoken = User.FindFirst("accessToken")?.Value ?? string.Empty;
		//	if (!await _tokenServices.IsTokenBlacklisted(accesstoken))
		//	{
		//		return Unauthorized();
		//	}
		//	string sid = User.FindFirst("SubjectID")?.Value;
		//	if (Guid.TryParse(sid, out Guid Id))
		//	{
		//		Guid gsid = Id;
		//		Guid glid = Guid.Empty;
		//		if (Guid.TryParse(lid, out Guid lguid))
		//		{
		//			glid = lguid;
		//		}
		//		else
		//			return BadRequest(new { message = "invalid lesson id" });
		//		ResultDTO res = await _teacherServices.DeleteLesson(gsid, glid, cancellationToken);
		//		return StatusCode(res.StatusCode, new { res.Message, res.result });
		//	}
		//	else
		//		return StatusCode(StatusCodes.Status400BadRequest, new { message = "You still have not been verified yet" });
		//}

		////public 

		[HttpPost("Add-words-to-Dictionary/{sid}")]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> addWordsToDictionary(string sid, [FromForm] AddVocabDTO vocabDTO, CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value;
			string uid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			//string sid = User.FindFirst("subjectID")?.Value;
			if (Guid.TryParse(uid, out Guid Uguid))
			{
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
						vocabDTO.Tid = Uguid;
						ResultDTO res = await _teacherServices.addWords(vocabDTO, cancellationToken);
						return StatusCode(res.StatusCode, new { res.Message });
					}
				}
				else
					return BadRequest(new { message = "Invalid subject ID" });
			}
			else
				return Unauthorized();
		}

		[HttpPost("Create-Exercise")]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> CreateExercise([FromForm] CreateExerciseDTO createExercise ,CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value;
			string Tid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
			if (Tid.IsNullOrEmpty())
				return Unauthorized();
			if (await _tokenServices.IsTokenBlacklisted(accessToken))
				return Unauthorized();
			if (Guid.TryParse(Tid, out Guid GTid))
			{
				createExercise.Tid = GTid;
				ResultDTO res = await _teacherServices.CreateExercise(createExercise, cancellationToken);
				return StatusCode(res.StatusCode, res.Message);
			}
			else
				return Unauthorized();

		}

		//implement add exercises

	}
}
