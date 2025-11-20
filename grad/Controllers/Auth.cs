using System.Security.Claims;
using grad.DTO;
using grad.Interfaces;
using grad.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace grad.Controllers
{
	[ApiController]
	[Route("Auth")]
	public class Auth : ControllerBase
	{
		private readonly IUserServices _userService;
		private readonly ITokenServices _tokenServices;
		public Auth(IUserServices userService, ITokenServices tokenServices)
		{
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
			_tokenServices = tokenServices ?? throw new ArgumentNullException(nameof(tokenServices));
		}
		[AllowAnonymous]
		[HttpPost("sign-in")]
		public async Task<IActionResult> login([FromBody] LoginDTO login,CancellationToken cancellationToken)//tested
		{
			if (!ModelState.IsValid || login == null)
			{ 
				return BadRequest(ModelState);
			}
			else
			{
				ResultDTO res = await _userService.login(login,cancellationToken);
				return StatusCode(res.StatusCode, new {Message = res.Message,Data = res.result});
			}
		}

		[AllowAnonymous]
		[HttpPost("Refresh-Token")]
		public async Task<IActionResult> RefreshToken([FromBody] RequestRefreshToken token,CancellationToken cancellationToken)//tested
		{
			token.AccessToken = Request.Headers.Authorization.ToString();
			if (!ModelState.IsValid || token.AccessToken.IsNullOrEmpty())
				return BadRequest(new
				{
					StatusCode = 400,
					Message = "Invalid Token"
				});
			ResultDTO res = await _userService.refreshToken(token, cancellationToken);
			return StatusCode(res.StatusCode, new { Message = res.Message, Data = res.result });
		}

		[AllowAnonymous]
		[HttpPost("Sign-Up")]
		public async Task<IActionResult> register([FromBody] SignupDTO user,CancellationToken cancellationToken)// implement teacher
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			else
			{
				ResultDTO result = await _userService.register(user, cancellationToken: cancellationToken);
				return StatusCode(result.StatusCode, new
				{
					Message = result.Message
				});
			}
		}


		[HttpPost("Request-Password-change")]
		public async Task<IActionResult> RPCHANGE(string EmailorUserName, CancellationToken cancellationToken)
		{
			User user = new User
			{
				EmailorUserName = EmailorUserName
			};
			ResultDTO result = await _userService.RequestChangePass(user, cancellationToken);
			return StatusCode(result.StatusCode, new
			{
				Message = result.Message
			});
		}

		[HttpPost("Change-Password-OTP")]
		public async Task<IActionResult> ResetPasswordOTP([FromBody] OTP_DTO otp, CancellationToken cancellationToken)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			ResultDTO result = await _userService.ResetPasswordOTP(otp, cancellationToken);
			return StatusCode(result.StatusCode, new
			{
				Message = result.Message
			});
		}

		[Authorize(Roles = "ResetPassword")]
		[HttpPost("reset-password")]
		public async Task<IActionResult> resetPassword(string newpass) //isnt completed yet when its done we need to configure the email right also dont forget to enable redis
		{
			var passwordRegex = new System.Text.RegularExpressions.Regex(
				@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$"
			);
			if (newpass.IsNullOrEmpty())
			{
				return StatusCode(StatusCodes.Status400BadRequest, new
				{
					Message = "Invalid credentials"
				});
			}
			else if (!passwordRegex.IsMatch(newpass))
			{
				return StatusCode(StatusCodes.Status400BadRequest, new
				{
					Message = "Password must be at least 8 characters long, contain upper and lower case letters, a number, and a special character."
				});
			}
			else
			{
				if (!await _tokenServices.IsTokenBlacklisted(Request.Headers.Authorization.ToString()))
				{
					string Id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
					if(string.IsNullOrEmpty(Id))
					{
						return Unauthorized(new { Message = "Invalid token" });
					}
					else
					{
						//string email = User.FindFirst(System.Security.Claims.ClaimTypes.Email).Value;
						ChangePasswordDTO resetPass = new ChangePasswordDTO
						{
							Id = Id,
							NewPassword = newpass,
							OldPassword = newpass,
							token = Request.Headers.Authorization.ToString()
						};
						ResultDTO result = await _userService.ResetPassword(resetPass);
						return StatusCode(result.StatusCode, new
						{
							Message = result.Message
						});
					}
				}
				else
				{
					return Forbid();
				}
			}
		}

		[Authorize(Roles = "Admin,Teacher,Student,Parent")]
		[HttpPost("change-password")]
		public async Task<IActionResult> changePassword(ChangePasswordDTO changePassword)//Tested
		{
			//_userService.ResetPassword(changePassword);
			if (!await _tokenServices.IsTokenBlacklisted(Request.Headers.Authorization.ToString()))
			{
				string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				if (string.IsNullOrEmpty(userId))
				{
					return Unauthorized(new { Message = "Invalid token" });
				}
				else
				{
					changePassword.Id = userId;
					//changePassword.token = Request.Headers.Authorization.ToString();
					ResultDTO result = await _userService.ResetPassword(changePassword);
					return StatusCode(result.StatusCode, new
					{
						Message = result.Message
					});
				}
			}
			else
			{
				return StatusCode(StatusCodes.Status401Unauthorized,
				new
				{
					Message = "Invalid token"
				});
			}
		}

		[Authorize(Roles = "Admin,Teacher,Student,Parent")]
		[HttpPost("logout")]
		public async Task<IActionResult> logout(CancellationToken cancellationToken)// needs mofication
		{
			string Id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
			if (string.IsNullOrEmpty(Id)) {
				return Unauthorized(new { Message = "Invalid token" });
			}
			ResultDTO res = await _userService.LogOut(Request.Headers.Authorization.ToString(),Id,cancellationToken);
			return StatusCode(res.StatusCode, new
			{
				Message = res.Message
			});
		}

	}
}
