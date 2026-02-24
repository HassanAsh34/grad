using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Grad_Structured.Application.Auth.DTOs
{
	public class LoginDTO
	{
		[EmailAddress(ErrorMessage = "Invalid Email")]
		[Required(ErrorMessage ="Email field is required")]
		public string UsernameorEmail { get; set; }

		//public string  { get; set; }
		[Required(ErrorMessage ="Password Field is required")]
		[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",ErrorMessage = "Password must be at least 8 characters long, contain upper and lower case letters, a number, and a special character.")]
		public string password { get; set; }
	}
}
