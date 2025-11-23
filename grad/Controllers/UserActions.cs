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

		[HttpPost("view-profile")]
		[Authorize(Roles = "Admin,Teacher,Student,Parent")]
		public async Task<IActionResult> viewProfile()//Tested
		{
			//bool NotAuth = await _tokenServices.IsTokenBlacklisted(Request.Headers.Authorization.ToString());
			if (!(await _tokenServices.IsTokenBlacklisted(Request.Headers.Authorization.ToString())))
			{
				var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
				var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role);
				if (idClaim == null || roleClaim == null)
				{
					return Unauthorized(new { Message = "Invalid token" });
				}
				ProfileDTO user = new ProfileDTO
				{
					Id = idClaim.Value,
					Role = roleClaim.Value,
				};
				ResultDTO res = await _userService.ViewProfile(user);
				return StatusCode(res.StatusCode, new
				{
					Message = res.Message,
					Data = res.result
				});
			}
			else
			{
				return Forbid();
			}
			//// Implementation for viewing profile goes here
			//return Ok(User.Claims.ToList());
		}





	}
}
