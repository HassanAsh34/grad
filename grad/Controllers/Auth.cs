using System.Security.Claims;
using grad.DTO;
using grad.Interfaces;
using grad.Model;
using grad.Services;
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
		//private readonly UserServices _userService;
		//private readonly TokenServices _tokenServices;
		public Auth(IUserServices userService, ITokenServices tokenServices)
		{
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
			_tokenServices = tokenServices ?? throw new ArgumentNullException(nameof(tokenServices));
		}
		[AllowAnonymous]
		[HttpPost("sign-in")]
		public async Task<IActionResult> login([FromBody] LoginDTO login,CancellationToken cancellationToken)
		{
			if (!ModelState.IsValid || login == null)
			{ 
				return BadRequest(ModelState);
			}
			else
			{
				ResultDTO res = await _userService.login(login,cancellationToken);
				if(res.result != null && res.result is ResponseTokenDTO token)
				{
					
					Response.Cookies.Append("access_token", token.AccessToken,new CookieOptions
					{
						HttpOnly = true,
						Secure = true,
						SameSite = SameSiteMode.Lax,
						Expires = DateTimeOffset.UtcNow.AddMinutes(15)
					});
					Response.Cookies.Append("refresh_token", token.RefreshToken, new CookieOptions
					{
						HttpOnly = true,           
						Secure = true,              
						SameSite = SameSiteMode.Strict, 
						Expires = DateTimeOffset.UtcNow.AddDays(7)
					});
					return Ok(res.Message);
				}
				else
				{
					return StatusCode(res.StatusCode, new {Message = res.Message});
				}
			}
		}


		[AllowAnonymous]
		[HttpPost("Refresh-Token")]
		public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken)
		{
			//ResponseTokenDTO tokenDTO = new ResponseTokenDTO
			//{
			//	AccessToken = Request.Cookies["access_token"],
			//	RefreshToken = Request.Cookies["refresh_token"]
			//};
			string refreshToken = Request.Cookies["refresh_token"];

			Console.WriteLine(refreshToken);
			//Console.WriteLine(tokenDTO.AccessToken);
			//Console.WriteLine(tokenDTO.RefreshToken);
			if (refreshToken.IsNullOrEmpty())
				return StatusCode(StatusCodes.Status401Unauthorized,
					new { Message = "Refresh token is invalid or expired. Please log in again." });
			else
			{
				ResultDTO res = await _userService.refreshToken(refreshToken, cancellationToken);
				if (res.result is ResponseTokenDTO tokenDTO)
				{
					Response.Cookies.Append("access_token", tokenDTO.AccessToken, new CookieOptions
					{
						HttpOnly = true,
						Secure = true,
						SameSite = SameSiteMode.Lax,
						Expires = DateTimeOffset.UtcNow.AddMinutes(15)
					});
					return Ok();
				}
				else
				{
					return StatusCode(res.StatusCode,
					new { Message = res.Message });
				}
			}
		}



		[AllowAnonymous]
		[HttpPost("Sign-Up")]
		[Consumes("multipart/form-data")] //add cloud storage instead of using local cloudinary
		public async Task<IActionResult> register([FromForm] SignupDTO user,CancellationToken cancellationToken) //fix refresh token
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			else
			{
				//we need to add a default picture for the users
				ResultDTO result = await _userService.register(user, cancellationToken: cancellationToken);
				if(result.StatusCode != StatusCodes.Status201Created)
					System.IO.File.Delete(user.filePath);
				return StatusCode(result.StatusCode, new
				{
					Message = result.Message
				});
			}
		}


		[AllowAnonymous]
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



		//	//[HttpPost]
		[AllowAnonymous]
		[HttpPost("Verify-OTP")]
		public async Task<IActionResult> CheckOTP([FromBody] OTP_DTO otp, CancellationToken cancellationToken)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			ResultDTO result = await _userService.ResetPasswordOTP(otp, cancellationToken);
			if (result.result is string s && !s.IsNullOrEmpty())
				Response.Cookies.Append("Reset_Token",s);
			return StatusCode(result.StatusCode,result.Message);
		}


		[Authorize(Roles = "ResetPassword")]
		[HttpPatch("reset-password")]
		public async Task<IActionResult> resetPassword(ChangePasswordDTO newpass,CancellationToken cancellationToken) //isnt completed yet when its done we need to configure the email right also dont forget to enable redis
		{
			/*var passwordRegex = new System.Text.RegularExpressions.Regex(
				@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#^()_+\-])[A-Za-z\d@$!%*?&#^()_+\-]{8,}$"
			);*/
			if (newpass.NewPassword.IsNullOrEmpty())
			{
				return StatusCode(StatusCodes.Status400BadRequest, new
				{
					Message = "Invalid credentials"
				});
			}
			//else if (!passwordRegex.IsMatch(newpass))
			//{
			//	return StatusCode(StatusCodes.Status400BadRequest, new
			//	{
			//		Message = "Password must be at least 8 characters long, contain upper and lower case letters, a number, and a special character."
			//	});
			//}
			else
			{
				string accsstoken = Request.Cookies["Reset_Token"];
				if (!await _tokenServices.IsTokenBlacklisted(accsstoken))
				{
					string Id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
					if (string.IsNullOrEmpty(Id))
					{
						return Unauthorized(new { Message = "Invalid token" });
					}
					else
					{
						//string email = User.FindFirst(System.Security.Claims.ClaimTypes.Email).Value;

						ChangePasswordDTO resetPass = new ChangePasswordDTO
						{
							Id = Id,
							NewPassword = newpass.NewPassword,
							token = Request.Headers.Authorization.ToString()
						};
						ResultDTO result = await _userService.ResetPassword(resetPass,true, cancellationToken);
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
		[HttpPatch("change-password")]
		public async Task<IActionResult> changePassword(ChangePasswordDTO changePassword,CancellationToken cancellationToken)//Tested
		{
			//_userService.ResetPassword(changePassword);
			changePassword.token = Request.Cookies["access_token"];
			changePassword.refreshToken = Request.Cookies["refresh_token"];
			if(!changePassword.token.IsNullOrEmpty()&&!changePassword.refreshToken.IsNullOrEmpty())
			{
				if (!await _tokenServices.IsTokenBlacklisted(changePassword.token))
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
						ResultDTO result = await _userService.ResetPassword(changePassword,cancellationToken: cancellationToken);
						return StatusCode(result.StatusCode, new
						{
							Message = result.Message
						});
					}
				}
				else
				{
					return Unauthorized(new { Message = "Invalid token" });
				}
			}
			else
			{
				return Unauthorized(new { Message = "Invalid token" });
			}
		}

		[Authorize(Roles = "Admin,Teacher,Student,Parent")]
		[HttpPatch("logout")]
		public async Task<IActionResult> logout(CancellationToken cancellationToken)// needs mofication
		{
			string accesstoken = Request.Cookies["access_token"];
			string refreshtoken = Request.Cookies["refresh_token"];
			if (string.IsNullOrEmpty(accesstoken)||refreshtoken.IsNullOrEmpty()) {
				return NoContent();
			}
			string Id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
			ResultDTO res = await _userService.LogOut(accesstoken,refreshtoken,Id,cancellationToken);
			return StatusCode(res.StatusCode, new
			{
				Message = res.Message
			});
		}

	}
}
