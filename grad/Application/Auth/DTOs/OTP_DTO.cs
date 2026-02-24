using System.ComponentModel.DataAnnotations;

namespace Grad_Structured.Application.Auth.DTOs
{
	public class OTP_DTO
	{
		public string EmailorUserName { get; set; }
		//[Required]
		//[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#^()_+\-])[A-Za-z\d@$!%*?&#^()_+\-]{8,}$",
		//ErrorMessage = "Password must be at least 8 characters, with uppercase, lowercase, digit, and special character.")]
		//public string NewPassword { get; set; }

		[Required]
		[StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 characters long.")]
		public string OTP { get; set; }
	}
}
