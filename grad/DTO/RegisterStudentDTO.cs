using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using grad.Model;

namespace grad.DTO
{
	public class RegisterStudentDTO
	{
		[JsonIgnore]
		public Guid ?p_Id { get; set; }

		public IFormFile ?image { get; set; }

		[JsonIgnore]
		public string ?Image_Path { get; set; }

		[EmailAddress(ErrorMessage = "Invalid Email Address")]
		public string ?Email { get; set; }

		[Required(ErrorMessage = "Password is required")]
		[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "Password must be at least 8 characters long, contain upper and lower case letters, a number, and a special character.")]
		public string password { get; set; }

		public string FName { get; set; }

		[JsonIgnore]
		public string ?Pname { get; set; }

		public DateOnly BirthDate { get; set; }

		

		public int ?gender { get; set; }

		public int Disability { get; set; } // 0: None, 1: Hearing, 2: Speech

		//public string Address { get; set; }

		[JsonIgnore]
		
		public string ?PEmail { get; set; }

		[JsonIgnore]

		public string ?Username { get; set; }

	}
}
