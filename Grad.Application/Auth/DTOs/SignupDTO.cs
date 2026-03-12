using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
//using Google.Type;

namespace Grad.Application.Auth.DTOs
{
	public class SignupDTO //finish the rest of validation
	{
		//public string username { get; set; }

		public IFormFile? file { get; set; }

		[JsonIgnore]
		public string? filePath { get; set; }

		[EmailAddress(ErrorMessage = "Invalid Email")]
		[Required(ErrorMessage = "Email field is required")]
		public string email { get; set; }
			
		public int role { get; set; }

		public int ?Gender { get; set; }

		[Required(ErrorMessage = "Password Field is required")]
		[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "Password must be at least 8 characters long, contain upper and lower case letters, a number, and a special character.")]
		public string password { get; set; }

		public DateOnly ?BirthDate { get; set; }

		public DateOnly BD => BirthDate == null ? DateOnly.FromDateTime(DateTime.UtcNow) : BirthDate.Value;

		public string Address { get; set; }

		//public string NationalID { get; set; }
		public int ?Disability { get; set ; } // 0: None, 1: Hearing, 2: Speech

		public string FName { get; set; }

		public string LName { get; set; }

		[Phone]
		public string phoneNumber { get; set; }

		public string ?Job { get; set; }

		public Guid ?SubjectID { get; set; }

	}
}
