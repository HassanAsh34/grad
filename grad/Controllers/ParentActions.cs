using grad.DTO;
using grad.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace grad.Controllers
{
	[ApiController]
	[Route("/Parent")]
	[Authorize(Roles = "Parent")]
	public class ParentActions : ControllerBase
	{
		private readonly IParentServices _parentServices;

		public ParentActions(IParentServices parentServices)
		{
			_parentServices = parentServices ?? throw new ArgumentNullException(nameof(parentServices));
		}


		[HttpPost("register-student")]
		public async Task<IActionResult> RegisterStudent(RegisterStudentDTO studentDTO,CancellationToken cancellationToken)
		{
			//studentDTO.
			string pid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			string email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

			string PName = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value;

			string PhoneNumber = User.FindFirst("Phone")?.Value;

			if (pid.IsNullOrEmpty() || email.IsNullOrEmpty() || PName.IsNullOrEmpty() || PhoneNumber.IsNullOrEmpty())
				return StatusCode(StatusCodes.Status400BadRequest, new { Message = "Invalid credentials" });
			else
			{
				studentDTO.p_Id = pid;
				studentDTO.PEmail = email;
				studentDTO.Pname = PName;
				ResultDTO res = await _parentServices.registerStudent(studentDTO, cancellationToken);
				return StatusCode(res.StatusCode, new
				{
					Message = res.Message
				});
			}

			}
		[Authorize(Roles = "Parent")]
		[HttpPost("Activate-student")]
		public async Task<IActionResult> ActivateStudent(LoginDTO login, CancellationToken cancellationToken)
		{
			var passwordRegex = new System.Text.RegularExpressions.Regex(
				@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$"
			);
			if (login.UsernameorEmail.IsNullOrEmpty() || login.password.IsNullOrEmpty())
			{
				return StatusCode(StatusCodes.Status400BadRequest, new { Message = "Invalid credentials" });
			}
			else if (!passwordRegex.IsMatch(login.password))
			{
				return StatusCode(StatusCodes.Status400BadRequest, new {
					Message = "Password must be at least 8 characters long, contain upper and lower case letters, a number, and a special character."
				});
			}
			else
			{
				ResultDTO res = await _parentServices.activateAccount(login, cancellationToken);
				return StatusCode(res.StatusCode, new
				{
					Message = res.Message
				});
			} 
		}

		[HttpPost("Show-children")]
		[Authorize(Roles = "Parent")]
		public async Task<IActionResult> ShowChildren(CancellationToken cancellationToken)
		{
			string pid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			if (pid.IsNullOrEmpty())
				return StatusCode(StatusCodes.Status400BadRequest, new { ErrMessage = "Invalid credentials" });
			else
			{
				ResultDTO res = await _parentServices.ShowChildren(pid,cancellationToken);
				return StatusCode(res.StatusCode, new
				{
					Message = res.Message,
					Data = res.result
				});
			}
		}



	}
}
