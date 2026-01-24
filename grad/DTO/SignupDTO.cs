using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace grad.DTO
{
	public class SignupDTO
	{
		//public string username { get; set; }

		public IFormFile file { get; set; }

		[JsonIgnore]
		public string? filePath { get; set; } 

		[Required]
		[EmailAddress]
		public string email { get; set; }
			
		public int role { get; set; }

		public int ?Gender { get; set; }

		[Required]
		[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#^()_+\-])[A-Za-z\d@$!%*?&#^()_+\-]{8,}$",
		ErrorMessage = "Password must be at least 8 characters, with uppercase, lowercase, digit, and special character.")]
		public string password { get; set; }


		public string Address { get; set; }

		//public string NationalID { get; set; }

		public string FName { get; set; }

		public string LName { get; set; }

		public string phoneNumber { get; set; }

		public string ?Job { get; set; }

		public string ?SubjectID { get; set; }

	}
}
