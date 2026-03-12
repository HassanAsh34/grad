using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Grad.Application.Auth.DTOs
{
	public class ChangePasswordDTO
	{
		[JsonIgnore]
		public Guid ?Id { get; set; }

		[JsonIgnore]
		public string ?refreshToken { get; set; }
		//[Required]
		[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#^()_+\-])[A-Za-z\d@$!%*?&#^()_+\-]{8,}$",
		ErrorMessage = "Password must be at least 8 characters, with uppercase, lowercase, digit, and special character.")]
		public string ?OldPassword { get; set; } // need to be enforced from the front end

		[Required]
		[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#^()_+\-])[A-Za-z\d@$!%*?&#^()_+\-]{8,}$",
		ErrorMessage = "Password must be at least 8 characters, with uppercase, lowercase, digit, and special character.")]
		public string NewPassword { get; set; }

		[JsonIgnore]
		public string ?token { get; set; }
	}
}
