using Grad.Application.Common.DTOs;
using Grad.Application.ParentFeatures.DTOs;
using Grad.Application.ParentFeatures.Interfaces;
using Grad.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace grad.API.Controllers
{
	[ApiController]
	[Route("/Parent")]
	[Authorize(Roles = "Parent")]
	public class ParentActions : ControllerBase
	{
		private readonly IParentServices _parentServices;
		private readonly ITokenServices _tokenServices;

		public ParentActions(IParentServices parentServices, ITokenServices tokenServices)
		{
			_parentServices = parentServices ?? throw new ArgumentNullException(nameof(parentServices));
			_tokenServices = tokenServices ?? throw new ArgumentNullException(nameof(parentServices));
		}


		[HttpPost("register-student")]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> RegisterStudent([FromForm] RegisterStudentDTO studentDTO,CancellationToken cancellationToken)
		{
			//studentDTO.
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			string pid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			string email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

			string PName = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value;

			string PhoneNumber = User.FindFirst("Phone")?.Value;

			if (string.IsNullOrEmpty(pid) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(PName) || string.IsNullOrEmpty(PhoneNumber))
				return Unauthorized();
			else
			{
				if(Guid.TryParse(pid,out Guid parentId))
				{
					studentDTO.p_Id = parentId;
				}
				else
				{
					return Unauthorized();
				}
				studentDTO.Email = studentDTO.Email != null ? studentDTO.Email : string.Empty;
				studentDTO.PEmail = email;
				studentDTO.Pname = PName;
				ResultDTO res = await _parentServices.registerStudent(studentDTO, cancellationToken);
				return StatusCode(res.StatusCode, new
				{
					Message = res.Message
				});
			}

		}
		
		
		
		
		//[HttpPost("Activate-student")]
		//public async Task<IActionResult> ActivateStudent(LoginDTO login, CancellationToken cancellationToken)
		//{
		//	string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
		//	if (!await _tokenServices.IsTokenBlacklisted(accessToken))
		//	{
		//		return Unauthorized();
		//	}
		//	if (!ModelState.IsValid)
		//	{
		//		return BadRequest(ModelState);
		//	}
		//	else
		//	{
		//		string pid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
		//		if (Guid.TryParse(pid, out Guid parentId))
		//		{
		//			ResultDTO res = await _parentServices.activateAccount(login, parentId, cancellationToken);
		//			return StatusCode(res.StatusCode, new
		//			{
		//				Message = res.Message
		//			});
		//		}
		//		else
		//		{
		//			return Unauthorized();
		//		}
		//	} 
		//}

		[HttpGet("Show-children")]
		//[Authorize(Roles = "Parent")]
		public async Task<IActionResult> ShowChildren(CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			string pid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (Guid.TryParse(pid, out Guid parentId))
			{
				ResultDTO res = await _parentServices.ShowChildren(parentId, cancellationToken);
				return StatusCode(res.StatusCode, new
				{
					Message = res.Message,
					Data = res.result
				});
			}
			else
			{
				return Unauthorized();
			}
		}

		[HttpGet("View-Profile/{sid}")]
		public async Task<IActionResult> ViewProfile(string sid, CancellationToken cancellationToken)
		{
			string accessToken = User.FindFirst("accessToken")?.Value ?? string.Empty;
			if (!await _tokenServices.IsTokenBlacklisted(accessToken))
			{
				return Unauthorized();
			}
			string pid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (Guid.TryParse(pid, out Guid parentId))
			{
				if(Guid.TryParse(sid, out Guid studentId))
				{
					ResultDTO result = await _parentServices.viewProfile(parentId, studentId, cancellationToken);
					return StatusCode(result.StatusCode, new
					{
						Message = result.Message,
						Data = result.result
					});
				}
				else
				{
					return BadRequest(new {message = "Invalid Student ID"});
				}
			}
			else
			{
				return Unauthorized();
			}
		}
	}
}
