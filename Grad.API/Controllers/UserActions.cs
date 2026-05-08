using System.Security.Claims;
using Grad.Application.Users.DTOs;
using Grad.Application.Users.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Grad.Application.Common.DTOs;
using Grad.Application.Common.Interfaces;



namespace Grad.API.Controllers
{
	[ApiController]
	[Route("Auth")]
	[Authorize(Roles = "Admin,Teacher,Student,Parent")]
	public class UserActions : ControllerBase
	{
		private readonly IUserServices _userService;
		private readonly ITokenServices _tokenServices;
		private readonly ILogger<UserActions> _logger;
		public UserActions(IUserServices userService, ITokenServices tokenServices, ILogger<UserActions> logger)
		{
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
			_tokenServices = tokenServices ?? throw new ArgumentNullException(nameof(tokenServices));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
				if (Guid.TryParse(idClaim,out Guid ID) && !string.IsNullOrEmpty(roleClaim))
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
					return Unauthorized();
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
			if (Guid.TryParse(id, out Guid ID) && !string.IsNullOrEmpty(roleClaim))
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

		[HttpDelete("Delete-profile")]
		public async Task<IActionResult> deleteProfile([FromHeader]bool all = false, CancellationToken cancellationToken = default)
		{
			string accesstoken = User.FindFirst("accessToken")?.Value;
			if (!await _tokenServices.IsTokenBlacklisted(accesstoken))
				return Unauthorized();
			string id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			string roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
			if (Guid.TryParse(id, out Guid ID) && !string.IsNullOrEmpty(roleClaim))
			{
				ResultDTO res = await _userService.DeleteProfile(new ProfileDTO { Id = ID, Role = roleClaim },null,all,false, cancellationToken);
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