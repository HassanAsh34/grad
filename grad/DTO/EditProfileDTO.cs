using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using grad.Model;

namespace grad.DTO
{
	public class EditProfileDTO
	{
		public Guid? Id { get; set; }


		public IFormFile ?image { get; set; }

		[MinLength(3, ErrorMessage ="First Name is too short. Minimum length is 3 characters")]
		public string? FName { get; set; }

		[MinLength(3, ErrorMessage = "Last Name is too short. Minimum length is 3 characters")]
		public string? lName { get; set; }

		[MinLength(10,ErrorMessage ="Invalide Home address")]
		public string? Address { get; set; }

		[Phone]
		public string? phone { get; set; }

		public string? Role { get; set; }

		//public DateOnly? BirthDate { get; set; }

		[MinLength(2,ErrorMessage ="Invalid Job title")]
		public string? Job { get; set; }
	}
}
