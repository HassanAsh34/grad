using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;


namespace Grad.Application.Users.DTOs
{
	public class EditProfileDTO
	{
		public Guid? Id { get; set; }

		public IFormFile? Image { get; set; }
		public string? ImagePath { get; set; }

		[MinLength(3, ErrorMessage = "First Name is too short. Minimum length is 3 characters")]
		public string? FName { get; set; }

		[MinLength(3, ErrorMessage = "Last Name is too short. Minimum length is 3 characters")]
		public string? lName { get; set; }

		[MinLength(10, ErrorMessage = "Invalid Home Address")]
		public string? Address { get; set; }

		[Phone]
		public string? phone { get; set; }

		public string? Role { get; set; }

		[MinLength(2, ErrorMessage = "Invalid Job title")]
		public string? Job { get; set; }
	}
}
