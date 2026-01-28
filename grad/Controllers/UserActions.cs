using System.Security.Claims;
using grad.DTO;
using grad.Interfaces;
using grad.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;


namespace grad.Controllers
{
	[ApiController]
	[Route("Auth")]
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
		[Authorize(Roles = "Admin,Teacher,Student,Parent")]
		public async Task<IActionResult> viewProfile()//Tested
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
					ResultDTO res = await _userService.ViewProfile(user);
					if (res.StatusCode == StatusCodes.Status200OK && res.result is ProfileDTO profile)
					{
						profile.pfpURL = $"{Request.Scheme}://{Request.Host}/{profile.pfpPath}";
						Console.WriteLine(profile.pfpURL);
					}
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





	}
}
