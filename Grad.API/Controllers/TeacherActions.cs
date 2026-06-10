using System.Security.Claims;
using System.Security.Cryptography;
using Azure.Core;
using Grad.Application.Common.DTOs;
using Grad.Application.Common.Interfaces;
using Grad.Application.ExerciseFeatures.DTOs;
using Grad.Application.LessonFeatures.DTOs;
using Grad.Application.SubjectFeatures.DTOs;
using Grad.Application.SubjectFeatures.Interfaces;
using Grad.Application.TeacherFeatures.DTOs;
using Grad.Application.TeacherFeatures.Interfaces;
using Grad.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Grad.API.Controllers
{
	[ApiController]
	[Route("/Teacher")]
	[Authorize(Roles = "Teacher")]
	public class TeacherActions : ControllerBase
	{
		//private readonly ISubjectServices _SubjectServices;
		private readonly ITokenServices _tokenServices;
		private readonly ITeacherServices _teacherServices;
		private readonly ILogger<TeacherActions> _logger;

		public TeacherActions(ISubjectServices subjectServices, ITokenServices tokenServices, ITeacherServices teacherServices, ILogger<TeacherActions> logger)
		{
			//_SubjectServices = subjectServices ?? throw new ArgumentNullException(nameof(subjectServices));
			_tokenServices = tokenServices ?? throw new ArgumentNullException(nameof(tokenServices));
			_teacherServices = teacherServices ?? throw new ArgumentNullException(nameof(teacherServices));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}


		//private readonly;
		[HttpGet("get-students/{sid}")]//done
		public async Task<IActionResult> getStudents(string sid, CancellationToken cancellation)
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
					return StatusCode(result.StatusCode, new { Message = result.Message, Data = result.result });
				}
				else
					return NotFound("Subject wasn't found");
			}
			else
				return Unauthorized();

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
				return StatusCode(res.StatusCode, new { Message = res.Message, Data = res.result });
			}
			else
				return NotFound(new { Message = "Student wasn't found" });
		}

		[HttpGet("Get-Student-Progress/{sid}/{stdID}")]
		public async Task<IActionResult> getStudentProgress(Guid sid,Guid stdID, CancellationToken cancellationToken)
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
				ResultDTO res = await _teacherServices.viewStudentProgress(stdID, new TeacherSubjectDTO()
                {
                    SubjectId = sid,
                    TeacherId = Id
                }, cancellationToken);
				return StatusCode(res.StatusCode, new { res.Message, res.result });
			}
			else
				return Unauthorized();
        }

		[HttpGet("Get-students-progress/{sid}")]
		public async Task<IActionResult> getStudentsProgress(Guid sid, CancellationToken cancellationToken)
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
                ResultDTO res = await _teacherServices.viewStudentsProgress(new TeacherSubjectDTO()
                {
                    SubjectId = sid,
                    TeacherId = Id
                }, cancellationToken);
                return StatusCode(res.StatusCode, new { res.Message, res.result });
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

		[HttpGet("view-subject/{sid}")]//done
		public async Task<IActionResult> viewSubject(string sid, CancellationToken cancellationToken)
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
					return StatusCode(res.StatusCode, new { message = res.Message, result = res.result });
				}
				else
					return StatusCode(StatusCodes.Status400BadRequest, new { Message = "Invalid subject ID" });
			}
			else
				return Unauthorized();
		}

		[HttpPost("Add-lesson")]//done//[Consumes("multipart/form-data")]//need to be fixed
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
					return BadRequest(new { ModelState });
				}
				ResultDTO res = await _teacherServices.AddLesson(lessonDTO, cancellationToken);
				return StatusCode(res.StatusCode, new { res.Message, res.result });
			}
			else
				return Unauthorized();
		}

		[HttpGet("get-lessons/{sid}")]//done
		public async Task<IActionResult> viewLessons(string sid, CancellationToken cancellationToken)
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
					return StatusCode(res.StatusCode, new { res.Message, res.result });
				}
				else
					return NotFound(new { Message = "No lessons were found" });
			}
			else
				return Unauthorized();

		}

		[HttpGet("View-Lesson/{Sid}/{Lid}")]//done 
		public async Task<IActionResult> viewLesson(string Sid, string Lid, CancellationToken cancellationToken)
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
				return NotFound(new { Message = "No lessons were found" });
			//}
			//else
			//	return StatusCode(StatusCodes.Status400BadRequest, new { message = "You still have not been verified yet" });
		}

		[HttpPatch("Edit-Lesson")]
		public async Task<IActionResult> editLesson([FromBody] EditLessonDTO lessonDTO, CancellationToken cancellationToken)
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

		[HttpDelete("Delete-Video")]
		public async Task<IActionResult> DeleteVideo([FromBody] VideoDTO videoDTO, CancellationToken cancellationToken)
		{
			string token = User.FindFirst("accessToken")?.Value;
			string Tid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
			if (string.IsNullOrEmpty(Tid))
				return Unauthorized();
			if (!await _tokenServices.IsTokenBlacklisted(token))
				return Unauthorized();
			if (Guid.TryParse(Tid, out Guid GTid))
			{
				ResultDTO res = await _teacherServices.DeleteVideo(videoDTO, GTid, cancellationToken);
				return StatusCode(res.StatusCode, new { res.Message, res.result });
			}
			else
				return Unauthorized();
		}


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
					return BadRequest(new { Message = "Invalid subject ID" });
			}
			else
				return Unauthorized();
		}


		[HttpPost("Create-Exercise")]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> CreateExercise([FromForm] CreateLevelDTO createExercise, CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value;
			string Tid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
			if (string.IsNullOrEmpty(Tid))
				return Unauthorized();
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
				return Unauthorized();
			if (Guid.TryParse(Tid, out Guid GTid))
			{
				createExercise.Tid = GTid;
				ResultDTO res = await _teacherServices.CreateExercise(createExercise, cancellationToken);
				return StatusCode(res.StatusCode, new { res.Message });
			}
			else
				return Unauthorized();

		}
		[HttpGet("subjects/{sid}/lessons/{lid}/levels/{levelId}")]
		public async Task<IActionResult> viewLessonLevel(Guid sid, Guid lid, Guid levelId, CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value;
			string Tid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
			if (string.IsNullOrEmpty(Tid))
				return Unauthorized();
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
				return Unauthorized();
			if (Guid.TryParse(Tid, out Guid GTid))
			{
				LevelDTO levelDTO = new LevelDTO
				{
					ID = levelId,
					Sid = sid,
					Lid = lid
				};
				ResultDTO result = await _teacherServices.ViewLevel(levelDTO, GTid, cancellationToken);
				return StatusCode(result.StatusCode, new { result.Message, result.result });
			}
			else
				return Unauthorized();
		}

		[HttpGet("List-Quizes/{sid}")]
		public async Task<IActionResult> ListQuizes(string sid, CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value;
			string Tid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
			if (string.IsNullOrEmpty(Tid))
				return Unauthorized();
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
				return Unauthorized();
			if (Guid.TryParse(Tid, out Guid GTid))
			{
				if (Guid.TryParse(sid, out Guid Id))
				{
					ResultDTO res = await _teacherServices.GetQuizes(new TeacherSubjectDTO { SubjectId = Id, TeacherId = GTid }, cancellationToken);
					return StatusCode(res.StatusCode, new { res.Message, res.result });
				}
				else
					return StatusCode(StatusCodes.Status400BadRequest, new { Message = "Invalid subject ID" });
			}
			else
				return Unauthorized();

		}



		[HttpGet("subjects/{sid}/levels/{levelId}")]
		public async Task<IActionResult> ViewQuiz(Guid sid, Guid levelId, CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value;
			string Tid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
			if (string.IsNullOrEmpty(Tid))
				return Unauthorized();
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
				return Unauthorized();
			if (Guid.TryParse(Tid, out Guid GTid))
			{
				LevelDTO levelDTO = new LevelDTO
				{
					ID = levelId,
					Sid = sid
				};
				ResultDTO result = await _teacherServices.ViewLevel(levelDTO, GTid, cancellationToken);
				return StatusCode(result.StatusCode, new { result.Message, result.result });
			}
			else
				return Unauthorized();
		}

		[HttpPatch("Edit-Level")]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> EditExercise([FromForm] EditLevelDTO editLevelDTO, CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value;
			string Tid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
			if (string.IsNullOrEmpty(Tid))
				return Unauthorized();
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
				return Unauthorized();
			if (Guid.TryParse(Tid, out Guid GTid))
			{
				editLevelDTO.Tid = GTid;
				ResultDTO res = await _teacherServices.EditLevel(editLevelDTO, cancellationToken);
				return StatusCode(res.StatusCode, new { Message = res.Message });
			}
			else
				return Unauthorized();

		}

		[HttpGet("list-Perquisites")] //NEEDS SOME ENHANCEMENT TO THE ENDPOINT
		public async Task<IActionResult> ListPerquisites([FromHeader] Guid sid, [FromHeader] PerquisiteType perquisiteType, CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value;
			string Tid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
			if (string.IsNullOrEmpty(Tid))
				return Unauthorized();
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
				return Unauthorized();
			if (Guid.TryParse(Tid, out Guid GTid))
			{
				ResultDTO res = await _teacherServices.ListPerquisites(new TeacherSubjectDTO { SubjectId = sid, TeacherId = GTid }, perquisiteType, cancellationToken);
				return StatusCode(res.StatusCode, new { res.Message, res.result });
			}
			else
				return Unauthorized();
		}
		//implement add exercises

		[HttpDelete("Delete-Level")]
		public async Task<IActionResult> DeleteExercise_quiz([FromBody] LevelDTO levelDTO, CancellationToken cancellationToken)
		{
			string token = User.FindFirst("accessToken")?.Value;
			string Tid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
			if (string.IsNullOrEmpty(Tid))
				return Unauthorized();
			if (!await _tokenServices.IsTokenBlacklisted(token))
				return Unauthorized();
			if (Guid.TryParse(Tid, out Guid GTid))
			{
				ResultDTO res = await _teacherServices.DeleteLevel(levelDTO, GTid, cancellationToken);
				return StatusCode(res.StatusCode, new { res.Message, res.result });
			}
			else
				return Unauthorized();
		}

		[HttpPatch("Update-CV")]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> updateCV([FromForm] UploadCVDTO upload) // we need to link it with frontend
		{
			string token = User.FindFirst("accessToken")?.Value;
			string Tid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
			//string email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? string.Empty;
			if (string.IsNullOrEmpty(Tid))
				return Unauthorized();
			if (!await _tokenServices.IsTokenBlacklisted(token))
				return Unauthorized();
			if (Guid.TryParse(Tid, out Guid GTid))
			{
				CVDTO cv = new CVDTO
				{
					CV = upload.file,
					Tid = GTid
				};
				ResultDTO res = await _teacherServices.uploadCV(cv);
				return StatusCode(res.StatusCode, new { res.Message, res.result });
			}
			else
				return Unauthorized();
		}


		[HttpGet("subjects/{sid}/Dictionary")]
		public async Task<IActionResult> ViewDictionary(Guid sid, CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value;
			string Tid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
			if (string.IsNullOrEmpty(Tid))
				return Unauthorized();
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
				return Unauthorized();
			if (Guid.TryParse(Tid, out Guid GTid))
			{
				TeacherSubjectDTO teacherSubjectDTO = new TeacherSubjectDTO
				{
					TeacherId = GTid,
					SubjectId = sid
				};
				ResultDTO result = await _teacherServices.ViewDictionary(teacherSubjectDTO, cancellationToken);
				return StatusCode(result.StatusCode, new { result.Message, result.result });
			}
			else
				return Unauthorized();
		}

		//[HttpDelete()] //add reomve video
	}
}