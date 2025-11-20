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


		[HttpPost("Show-Users")]
		public async Task<IActionResult> ShowUsers(CancellationToken cancellationToken)
		{
			ResultDTO res = await _adminServices.GetAllUsers(cancellationToken);
			return StatusCode(res.StatusCode, new { Message = res.Message, Data = res.result });
		}

		[HttpPost("view-user")]
		public async Task<IActionResult> ViewUser(ProfileDTO profileDTO,CancellationToken cancellationToken)
		{
			if (profileDTO.Id.IsNullOrEmpty() && profileDTO.Role.IsNullOrEmpty() && profileDTO != null) {
				return Unauthorized(new { Message = "Invalid User" });
			}
			else
			{
				ResultDTO res = await _adminServices.ViewUser(profileDTO,cancellationToken);
				return StatusCode(res.StatusCode, new { Message = res.Message, Data = res.result });
			}
		}

		[HttpPost("End-Session")]
		public async Task<IActionResult> EndSession(ProfileDTO profileDTO, CancellationToken cancellationToken)
		{
			if (profileDTO.Id.IsNullOrEmpty() && profileDTO.Role.IsNullOrEmpty() && profileDTO != null)
			{
				return Unauthorized(new { Message = "Invalid User" });
			}
			else
			{
				ResultDTO res = await _adminServices.EndSession(profileDTO, cancellationToken);
				return StatusCode(res.StatusCode, new { Message = res.Message });
			}
		}
		[HttpPost("Block-User")]
		public async Task<IActionResult> BlockUser(ProfileDTO profileDTO, CancellationToken cancellationToken)
		{
			if (profileDTO.Id.IsNullOrEmpty() && profileDTO.Role.IsNullOrEmpty() && profileDTO != null)
			{
				return Unauthorized(new { Message = "Invalid User" });
			}
			else
			{
				ResultDTO res = await _adminServices.BlockUser(profileDTO, cancellationToken);
				return StatusCode(res.StatusCode, new { Message = res.Message });
			}
		}

		[HttpPost("Unblock-User")]
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

		[HttpPost("List-Subjects")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> ListSubject(SubjectDTO subject,CancellationToken cancellationToken)
		{
			return BadRequest();
		}

		[HttpPost("Add-Subject")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> AddSubject(SubjectDTO subject, CancellationToken cancellationToken)
		{
			return BadRequest();
		}

		[HttpPost("Edit-Subject")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> EditSubject(SubjectDTO subject, CancellationToken cancellationToken)
		{
			return BadRequest();
		}

		[HttpPost("Remove-Subject")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> RemoveSubject(SubjectDTO subject, CancellationToken cancellationToken)
		{
			return BadRequest();
		}


		[HttpPost("Approve-teacher")]
		public async Task<IActionResult> ApproveTeacher(ProfileDTO profileDTO, CancellationToken cancellationToken)
		{
			if (profileDTO.Id.IsNullOrEmpty() && profileDTO.Role.IsNullOrEmpty() && profileDTO != null)
			{
				return Unauthorized(new { Message = "Invalid User" });
			}
			else
			{
				ResultDTO res = await _adminServices.ActivateTeacher(profileDTO, cancellationToken);
				return StatusCode(res.StatusCode, new { Message = res.Message });
			}
		}
	}
}
