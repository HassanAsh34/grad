using System.ComponentModel.DataAnnotations;

namespace grad.DTO
{
	public class LoginDTO
	{
		[Required]
		public string UsernameorEmail { get; set; }

		//public string  { get; set; }
		[Required]
		public string password { get; set; }
	}
}
