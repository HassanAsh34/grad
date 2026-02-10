using grad.DTO;
using grad.Interfaces;
using grad.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace grad.Controllers
{
	[ApiController]
	[Route("/Admin")]
	[Authorize(Roles = "Admin")]
	public class AdminActions : ControllerBase
	{
		private readonly IAdminServices _adminServices;
		private readonly ITokenServices _tokenServices;
		public AdminActions(IAdminServices adminServices, ITokenServices tokenServices)
		{
			_adminServices = adminServices ?? throw new ArgumentNullException(nameof(adminServices));
			_tokenServices = tokenServices ?? throw new ArgumentNullException(nameof(tokenServices));
		}


		//[HttpPost("Show-Active-sessions")]

		//public async Task<IActionResult> ShowActiveSessions(CancellationToken cancellationToken)
		//{
		//	throw new NotImplementedException();
		//}


		[HttpGet("Show-Users")]
		public async Task<IActionResult> ShowUsers(CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			ResultDTO res = await _adminServices.GetAllUsers(Request.Scheme,$"{Request.Host}",cancellationToken);
			return StatusCode(res.StatusCode, new { Message = res.Message, Data = res.result });
		}

		//[HttpPost("view-user")]
		//public async Task<IActionResult> ViewUser(ProfileDTO profileDTO, CancellationToken cancellationToken)
		//{
		//	if (profileDTO.Id.IsNullOrEmpty() && profileDTO.Role.IsNullOrEmpty() && profileDTO != null)
		//	{
		//		return Unauthorized(new { Message = "Invalid User" });
		//	}
		//	else
		//	{
		//		ResultDTO res = await _adminServices.ViewUser(profileDTO, cancellationToken);
		//		if (res.StatusCode == StatusCodes.Status200OK && res.result is ProfileDTO profile)
		//		{
		//			profile.pfpURL = $"{Request.Scheme}://{Request.Host}/{profile.pfpPath}";
		//			Console.WriteLine(profile.pfpURL);
		//		}
		//		return StatusCode(res.StatusCode, new { Message = res.Message, Data = res.result });
		//	}
		//}

		[HttpGet("view-user/{id}")]
		public async Task<IActionResult> ViewUser(string id, [FromQuery] string role, CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			if(Guid.TryParse(id, out Guid guid))
			{
				var dto = new ProfileDTO { Id = guid, Role = role };
				ResultDTO res = await _adminServices.ViewUser(dto, cancellationToken);
				if (res.StatusCode == StatusCodes.Status200OK && res.result is ProfileDTO profile)
				{
					//profile.pfpURL = $"{Request.Scheme}://{Request.Host}/{profile.pfpPath}";
					profile.pfpURL = profile.pfpPath;
				}
				return StatusCode(res.StatusCode, new { Message = res.Message, Data = res.result });
			}
			else
			{
				return Unauthorized(new { Message = "Invalid User" });
			}

		}

		//[HttpGet("view-user/{id}")]
		//public async Task<IActionResult> ViewUser(string id, CancellationToken cancellationToken)
		//{
		//	if (id.IsNullOrEmpty())
		//	{
		//		return Unauthorized(new { Message = "Invalid User" });
		//	}
		//	else
		//	{
		//		ResultDTO res = await _adminServices.ViewUser(id, cancellationToken);
		//		if (res.StatusCode == StatusCodes.Status200OK && res.result is ProfileDTO profile)
		//		{
		//			profile.pfpURL = $"{Request.Scheme}://{Request.Host}/{profile.pfpPath}";
		//			Console.WriteLine(profile.pfpURL);
		//		}
		//		return StatusCode(res.StatusCode, new { Message = res.Message, Data = res.result });
		//	}
		//}


		[HttpPatch("End-Session/{id}")]
		public async Task<IActionResult> EndSession(string id, CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			if (Guid.TryParse(id,out Guid guid))
			{
				ResultDTO res = await _adminServices.EndSession(guid, cancellationToken);
				return StatusCode(res.StatusCode, new { Message = res.Message });
			}
			else
			{
				return Unauthorized(new { Message = "Invalid User" });
			}
		}
		[HttpPatch("Toggle-Ban-User/{id}")]
		public async Task<IActionResult> BanUser(string id, CancellationToken cancellationToken) // need to be updated to toggle block/unblock
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			if (Guid.TryParse(id, out Guid guid))
			{
				ResultDTO res = await _adminServices.ToggleBan(guid, cancellationToken);
				return StatusCode(res.StatusCode);
			}
			else
			{
				return Unauthorized(new { Message = "Invalid User" });
			}
		}

		//[HttpPatch("Unblock-User")]
		//public async Task<IActionResult> UnblockUser(ProfileDTO profileDTO, CancellationToken cancellationToken) //not working yet
		//{
		//	string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
		//	if (!await _tokenServices.IsTokenBlacklisted(accessToken))
		//	{
		//		return Unauthorized();
		//	}
		//	if (profileDTO.Id.IsNullOrEmpty() && profileDTO.Role.IsNullOrEmpty() && profileDTO != null)
		//	{
		//		return Unauthorized(new { Message = "Invalid User" });
		//	}
		//	else
		//	{
		//		throw new NotImplementedException();
		//	}
		//}

		[HttpGet("List-Subjects")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> ListSubject(CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			ResultDTO res = await _adminServices.ViewSubjects(cancellationToken);
			return StatusCode(res.StatusCode, new { Message = res.Message, Data = res.result });
		}

		[HttpGet("View-Subject/{sid}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> ViewSubject(string sid,CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			if (Guid.TryParse(sid, out Guid guid))
			{
				ResultDTO res = await _adminServices.ViewSubject(guid, cancellationToken);
				return StatusCode(res.StatusCode, new { Message = res.Message, Data = res.result });
			}
			else
			{
				return StatusCode(400, new { Message = "invalid subject" });
			}
		}


		[HttpPost("Add-Subject")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> AddSubject(SubjectDTO subject, CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			if (!ModelState.IsValid) 
			{
				return StatusCode(400, new { Message = ModelState });
			}
			else
			{
				ResultDTO res = await _adminServices.AddSubject(subject, cancellationToken);
				return StatusCode(res.StatusCode,new {Message = res.Message,Data = res.result});
			}
		}

		[HttpPatch("Edit-Subject/{sid}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> EditSubject(SubjectDTO subject, CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			if (subject != null)
			{
				if (subject.SubjectId != null)
				{
					return BadRequest();
				}
				else
				{
					throw new NotImplementedException();
				}
			}
			return BadRequest();
		} //not implemented yet

		[HttpDelete("Remove-Subject/{sid}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> RemoveSubject(string sid, CancellationToken cancellationToken) //not implemented yet
		{
			throw new NotImplementedException();
		}


		[HttpPatch("Approve-teacher")]
		public async Task<IActionResult> ApproveTeacher(AssignTeacherDTO teacherDTO, CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			if (!ModelState.IsValid)
			{
				return StatusCode(StatusCodes.Status400BadRequest, new { Message = "Invalid user or subject" });
			}
			else
			{
				ResultDTO res = await _adminServices.AssignTeacherToSubject(teacherDTO, cancellationToken);
				return StatusCode(res.StatusCode, new { Message = res.Message });
			}
		}
	}
}
