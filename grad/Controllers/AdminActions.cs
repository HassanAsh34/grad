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
		public AdminActions(IAdminServices adminServices)
		{
			_adminServices = adminServices ?? throw new ArgumentNullException(nameof(adminServices));
		}


		//[HttpPost("Show-Active-sessions")]

		//public async Task<IActionResult> ShowActiveSessions(CancellationToken cancellationToken)
		//{
		//	throw new NotImplementedException();
		//}


		[HttpGet("Show-Users")]
		public async Task<IActionResult> ShowUsers(CancellationToken cancellationToken)
		{
			ResultDTO res = await _adminServices.GetAllUsers(cancellationToken);
			return StatusCode(res.StatusCode, new { Message = res.Message, Data = res.result });
		}

		[HttpPost("view-user")]
		public async Task<IActionResult> ViewUser(ProfileDTO profileDTO, CancellationToken cancellationToken)
		{
			if (profileDTO.Id.IsNullOrEmpty() && profileDTO.Role.IsNullOrEmpty() && profileDTO != null)
			{
				return Unauthorized(new { Message = "Invalid User" });
			}
			else
			{
				ResultDTO res = await _adminServices.ViewUser(profileDTO, cancellationToken);
				if (res.StatusCode == StatusCodes.Status200OK && res.result is ProfileDTO profile)
				{
					profile.pfpURL = $"{Request.Scheme}://{Request.Host}/{profile.pfpPath}";
					Console.WriteLine(profile.pfpURL);
				}
				return StatusCode(res.StatusCode, new { Message = res.Message, Data = res.result });
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
			if (id.IsNullOrEmpty())
			{
				return Unauthorized(new { Message = "Invalid User" });
			}
			else
			{
				ResultDTO res = await _adminServices.EndSession(id, cancellationToken);
				return StatusCode(res.StatusCode, new { Message = res.Message });
			}
		}
		[HttpPatch("Block-User/{id}")]
		public async Task<IActionResult> BlockUser(string id, CancellationToken cancellationToken)
		{
			if (id.IsNullOrEmpty())
			{
				return Unauthorized(new { Message = "Invalid User" });
			}
			else
			{
				ResultDTO res = await _adminServices.BlockUser(id, cancellationToken);
				return StatusCode(res.StatusCode, new { Message = res.Message });
			}
		}

		[HttpPatch("Unblock-User")]
		public async Task<IActionResult> UnblockUser(ProfileDTO profileDTO, CancellationToken cancellationToken) //not working yet
		{
			if (profileDTO.Id.IsNullOrEmpty() && profileDTO.Role.IsNullOrEmpty() && profileDTO != null)
			{
				return Unauthorized(new { Message = "Invalid User" });
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		[HttpGet("List-Subjects")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> ListSubject(CancellationToken cancellationToken)
		{
			ResultDTO res = await _adminServices.ViewSubjects(cancellationToken);
			return StatusCode(res.StatusCode, new { Message = res.Message, Data = res.result });
		}

		[HttpGet("View-Subject/{sid}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> ViewSubject(string sid,CancellationToken cancellationToken)
		{
			if(sid.IsNullOrEmpty())
			{
				return StatusCode(400, new { Message = "invalid subject" });
			}
			ResultDTO res = await _adminServices.ViewSubject(sid, cancellationToken);
			return StatusCode(res.StatusCode, new { Message = res.Message, Data = res.result });
		}


		[HttpPost("Add-Subject")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> AddSubject(SubjectDTO subject, CancellationToken cancellationToken)
		{
			if(!ModelState.IsValid) 
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
			if (subject != null)
			{
				if (subject.SubjectId.IsNullOrEmpty())
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
			if(!ModelState.IsValid)
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
