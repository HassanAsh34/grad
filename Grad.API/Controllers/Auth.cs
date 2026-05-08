using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Grad.Application.Auth.DTOs;
using Grad.Application.Auth.Interfaces;
using Grad.Application.Common.DTOs;
using Grad.Application.Common.Interfaces;
using Grad.Domain.Model;

//using grad.Application.Users.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Grad.API.Controllers
{
	[ApiController]
	[Route("Auth")]
	public class Auth : ControllerBase
	{
		private readonly IAuthServices _authServices;
		private readonly ITokenServices _tokenServices;
		private readonly ILogger<Auth> _logger;
		//private readonly UserServices _authServices;
		//private readonly TokenServices _tokenServices;
		public Auth(IAuthServices userService, ITokenServices tokenServices, ILogger<Auth> logger)
		{
			_authServices = userService ?? throw new ArgumentNullException(nameof(userService));
			_tokenServices = tokenServices ?? throw new ArgumentNullException(nameof(tokenServices));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}
		[AllowAnonymous]
		[HttpPost("sign-in")]
		public async Task<IActionResult> login([FromBody] LoginDTO login,CancellationToken cancellationToken)
		{
				ResultDTO res = await _authServices.login(login,cancellationToken);
				if(res.result != null && res.result is ResponseTokenDTO token)
				{

					//for testing purposes
					bool isHttps = Request.IsHttps; // Detect if the request is HTTPS

					//Response.Cookies.Append("access_token", token.AccessToken, new CookieOptions
					//{
					//	HttpOnly = true,
					//	Secure = isHttps, // Only secure if HTTPS
					//	SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax, // Lax works on HTTP
					//	Expires = DateTimeOffset.UtcNow.AddMinutes(15),
					//	Path = "/"
					//});


					//Response.Cookies.Append("refresh_token", token.RefreshToken, new CookieOptions
					//{
					//	HttpOnly = true,
					//	Secure = isHttps,
					//	SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
					//	Expires = DateTimeOffset.UtcNow.AddDays(7),
					//	Path = "/"
					//});


					//for docker and production we use this when both backend and frontend are using https
					Response.Cookies.Append("access_token", token.AccessToken, new CookieOptions
					{
						HttpOnly = true,
						Secure = true,
						SameSite = SameSiteMode.None,
						Expires = DateTimeOffset.UtcNow.AddMinutes(15),
						Path = "/"
					});

					Response.Cookies.Append("refresh_token", token.RefreshToken, new CookieOptions
					{
						HttpOnly = true,
						Secure = true,
						SameSite = SameSiteMode.None,
						Expires = DateTimeOffset.UtcNow.AddDays(7),
						Path = "/"
					});
					return Ok(res.Message);
				}
				else
				{
					return StatusCode(res.StatusCode, new {Message = res.Message});
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

			_logger.LogDebug("Refresh token request received, token present: {HasToken}", !string.IsNullOrEmpty(refreshToken));
			//Console.WriteLine(tokenDTO.AccessToken);
			//Console.WriteLine(tokenDTO.RefreshToken);
			if (string.IsNullOrEmpty(refreshToken))
				return StatusCode(StatusCodes.Status401Unauthorized,
					new { Message = "Refresh token is invalid or expired. Please log in again." });
			else
			{
				ResultDTO res = await _authServices.refreshToken(refreshToken, cancellationToken);
				if (res.result is ResponseTokenDTO tokenDTO)
				{
					//bool isHttps = Request.IsHttps;
					//Response.Cookies.Append("access_token", tokenDTO.AccessToken, new CookieOptions
					//{
					//	HttpOnly = true,
					//	Secure = isHttps, // Only secure if HTTPS
					//	SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax, // Lax works on HTTP
					//	Expires = DateTimeOffset.UtcNow.AddMinutes(15),
					//	Path = "/"
					//});
					//for docker and production we use this when both backend and frontend are using https
					Response.Cookies.Append("access_token", tokenDTO.AccessToken, new CookieOptions
					{
						HttpOnly = true,
						Secure = true,
						SameSite = SameSiteMode.None,
						Expires = DateTimeOffset.UtcNow.AddMinutes(15),
						Path = "/"
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
				ResultDTO result = await _authServices.register(user, cancellationToken: cancellationToken);
				//if(result.StatusCode != StatusCodes.Status201Created)
				//{
				//	if (!string.IsNullOrEmpty(user.filePath) && System.IO.File.Exists(user.filePath))
				//					System.IO.File.Delete(user.filePath);
				//}
				return StatusCode(result.StatusCode, new
				{
					Message = result.Message
				});
			}
		}


		[AllowAnonymous]
		[HttpPost("Request-Password-change/{Email}")]
		public async Task<IActionResult> RPCHANGE(
		[FromRoute]
		[RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format")]
		string Email,
		CancellationToken cancellationToken)
		{
			ResultDTO result = await _authServices.RequestChangePass(Email, cancellationToken);

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
			ResultDTO result = await _authServices.ResetPasswordOTP(otp, cancellationToken);
			if (result.result is string s && !string.IsNullOrEmpty(s) && result.StatusCode == StatusCodes.Status200OK)
			{
				bool isHttps = Request.IsHttps;
				Response.Cookies.Append("reset_token", s, new CookieOptions
				{
					HttpOnly = true,
					Secure = isHttps, // Only secure if HTTPS
					SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax, // Lax works on HTTP
					Expires = DateTimeOffset.UtcNow.AddMinutes(15),
					Path = "/"
				});
			}
			return StatusCode(result.StatusCode,result.Message);
		}


		[Authorize(Roles = "ResetPassword")]
		[HttpPatch("reset-password")]
		public async Task<IActionResult> resetPassword(ChangePasswordDTO newpass,CancellationToken cancellationToken) //isnt completed yet when its done we need to configure the email right also dont forget to enable redis
		{
			if (string.IsNullOrEmpty(newpass.NewPassword))
			{
				return StatusCode(StatusCodes.Status400BadRequest, new
				{
					Message = "Invalid credentials"
				});
			}
			else
			{
				string accsstoken = Request.Cookies["reset_token"];
				if (!await _tokenServices.IsTokenBlacklisted(accsstoken))
				{
					string Id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
					if(Guid.TryParse(Id, out Guid guid))
					{
						ChangePasswordDTO resetPass = new ChangePasswordDTO
						{
							Id = guid,
							NewPassword = newpass.NewPassword,
							token = Request.Headers["Authorization"]
						};
						ResultDTO result = await _authServices.ResetPassword(resetPass, true, cancellationToken);
						if (result.StatusCode == StatusCodes.Status200OK)
						{
							var expiredOptions = new CookieOptions
							{
								HttpOnly = true,
								Secure = Request.IsHttps,
								SameSite = Request.IsHttps ? SameSiteMode.None : SameSiteMode.Lax,
								Path = "/", // MUST match the Path used when setting
								Expires = DateTimeOffset.UtcNow.AddDays(-1)
							};

							Response.Cookies.Append("access_token", string.Empty, expiredOptions);
							Response.Cookies.Append("refresh_token", string.Empty, expiredOptions);
							Response.Cookies.Append("reset_token", string.Empty, expiredOptions);
							Response.Cookies.Delete("access_token", expiredOptions);
							Response.Cookies.Delete("refresh_token", expiredOptions);
							Response.Cookies.Delete("reset_token", expiredOptions);
						}
						return StatusCode(result.StatusCode, new
						{
							Message = result.Message
						});
					}
					else
					{
						return Unauthorized();
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
			//_authServices.ResetPassword(changePassword);
			changePassword.token = Request.Cookies["access_token"];
			changePassword.refreshToken = Request.Cookies["refresh_token"];
			if(!string.IsNullOrEmpty(changePassword.token)&&!string.IsNullOrEmpty(changePassword.refreshToken))
			{
				if (!await _tokenServices.IsTokenBlacklisted(changePassword.token))
				{
					string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
					
					if(Guid.TryParse(userId, out Guid guid))
					{
						changePassword.Id = guid;
						ResultDTO result = await _authServices.ResetPassword(changePassword, cancellationToken: cancellationToken);
						if (result.StatusCode == StatusCodes.Status200OK)
						{
							var expiredOptions = new CookieOptions
							{
								HttpOnly = true,
								Secure = Request.IsHttps,
								SameSite = Request.IsHttps ? SameSiteMode.None : SameSiteMode.Lax,
								Path = "/", // MUST match the Path used when setting
								Expires = DateTimeOffset.UtcNow.AddDays(-1)
							};

							Response.Cookies.Append("access_token", string.Empty, expiredOptions);
							Response.Cookies.Append("refresh_token", string.Empty, expiredOptions);
							Response.Cookies.Append("reset_token", string.Empty, expiredOptions);
							Response.Cookies.Delete("access_token", expiredOptions);
							Response.Cookies.Delete("refresh_token", expiredOptions);
							Response.Cookies.Delete("reset_token", expiredOptions);
						}
						return StatusCode(result.StatusCode, new
						{
							Message = result.Message
						});
					}
					else
					{
						return Unauthorized();
					}
				}
				else
				{
					return Unauthorized();
				}
			}
			else
			{
				return Unauthorized();
			}
		}

		[AllowAnonymous]
		[HttpPost("logout")]
		public async Task<IActionResult> logout(CancellationToken cancellationToken)
		{
			string accesstoken = Request.Cookies["access_token"];
			string refreshtoken = Request.Cookies["refresh_token"];
			
			// 1. ALWAYS clear cookies on the client side, regardless of token validity
			var expiredOptions = new CookieOptions
			{
				HttpOnly = true,
				Secure = Request.IsHttps,
				SameSite = Request.IsHttps ? SameSiteMode.None : SameSiteMode.Lax,
				Path = "/",
				Expires = DateTimeOffset.UtcNow.AddDays(-1)
			};
			Response.Cookies.Append("access_token", string.Empty, expiredOptions);
			Response.Cookies.Append("refresh_token", string.Empty, expiredOptions);
			Response.Cookies.Delete("access_token", expiredOptions);
			Response.Cookies.Delete("refresh_token", expiredOptions);

			LogoutDTO logoutDTO = new LogoutDTO
			{
				RefreshToken = refreshtoken,
				accessToken = accesstoken,
				RemainingTimeAcc = 20
			};
			
			ResultDTO res = await _authServices.LogOut(logoutDTO, cancellationToken);
			return StatusCode(res.StatusCode, new { Message = res.Message });
		}

	}
}
