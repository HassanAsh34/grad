using System.Security.Claims;
using Grad_Structured.Application.Users.DTOs;
using Grad_Structured.Application.Users.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Grad_Structured.Application.Common.DTOs;
using Grad_Structured.Application.Common.Interfaces;


namespace grad.Controllers
{
	[ApiController]
	[Route("Auth")]
	[Authorize(Roles = "Admin,Teacher,Student,Parent")]
	public class UserActions : ControllerBase
	{
		private readonly IUserServices _userService;
		private readonly ITokenServices _tokenServices;
		public UserActions(IUserServices userService, ITokenServices tokenServices)
		{
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
			_tokenServices = tokenServices ?? throw new ArgumentNullException(nameof(tokenServices));
		}

		[HttpGet("view-profile")]
		
		public async Task<IActionResult> viewProfile(CancellationToken cancellationToken)//Tested
		{
			//bool NotAuth = await _tokenServices.IsTokenBlacklisted(Request.Headers.Authorization.ToString());
			string token = Request.Cookies["access_token"];
			if (!(await _tokenServices.IsTokenBlacklisted(token)))
			{
				string idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
				string roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
				if (Guid.TryParse(idClaim,out Guid ID) && !roleClaim.IsNullOrEmpty())
				{
					ProfileDTO user = new ProfileDTO
					{
						Id = ID,
						Role = roleClaim,
					};
					ResultDTO res = await _userService.ViewProfile(user,cancellationToken: cancellationToken);
					//if (res.StatusCode == StatusCodes.Status200OK && res.result is ProfileDTO profile)
					//{
					//	profile.pfpURL = string.Empty;
					//	if (!profile.pfpPath.IsNullOrEmpty())
					//		profile.pfpURL = profile.pfpPath;
					//	//profile.pfpURL = $"{Request.Scheme}://{Request.Host}/{profile.pfpPath}";	
					//	Console.WriteLine(profile.pfpURL);
					//}
					return StatusCode(res.StatusCode, new
					{
						Message = res.Message,
						Data = res.result
					});
				}
				else
					return Unauthorized(new { Message = "Invalid token" });
			}
			else
			{
				return Unauthorized();
			}
			//// Implementation for viewing profile goes here
			//return Ok(User.Claims.ToList());
		}

		[HttpPatch("Edit-profile")]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> editProfile([FromForm] EditProfileDTO editProfile, CancellationToken cancellationToken)
		{
			string accesstoken = User.FindFirst("accessToken")?.Value;
			if (!await _tokenServices.IsTokenBlacklisted(accesstoken))
				return Unauthorized();
			string id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			string roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
			if (Guid.TryParse(id, out Guid ID) && !roleClaim.IsNullOrEmpty())
			{
				editProfile.Id = ID;
				editProfile.Role = roleClaim;
				ResultDTO res = await _userService.EditProfile(editProfile, cancellationToken);
				return StatusCode(res.StatusCode, new
				{
					Message = res.Message
				});
			}
			else
				return Unauthorized();
		}
	}
}
