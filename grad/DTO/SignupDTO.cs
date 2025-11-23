using System.ComponentModel.DataAnnotations;

namespace grad.DTO
{
	public class SignupDTO
	{
		//public string username { get; set; }

		[Required]
		[EmailAddress]
		public string email { get; set; }

		public int role { get; set; }

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
