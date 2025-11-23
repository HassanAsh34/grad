using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace grad.DTO
{
	public class ChangePasswordDTO
	{
		[JsonIgnore]
		public string ?Id { get; set; }
		[Required]
		[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#^()_+\-])[A-Za-z\d@$!%*?&#^()_+\-]{8,}$",
		ErrorMessage = "Password must be at least 8 characters, with uppercase, lowercase, digit, and special character.")]
		public string OldPassword { get; set; }

		[Required]
		[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#^()_+\-])[A-Za-z\d@$!%*?&#^()_+\-]{8,}$",
		ErrorMessage = "Password must be at least 8 characters, with uppercase, lowercase, digit, and special character.")]
		public string NewPassword { get; set; }

		[JsonIgnore]
		public string ?token { get; set; }
	}
}
