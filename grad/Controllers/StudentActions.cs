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
				ResultDTO result = await _studentServices.ViewSubjects(guid, cancellationToken);
				return StatusCode(result.StatusCode, new { message = result.Message, result = result });
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
				return StatusCode(result.StatusCode, new { message = result.Message, result = result });
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
		//[HttpGet("View-Lessons")]
		//view lesson

		//view enrolled subjects
	}
}
