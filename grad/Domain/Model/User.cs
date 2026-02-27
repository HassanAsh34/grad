using System.ComponentModel.DataAnnotations;
using grad.Domain.Enums;

namespace grad.Domain.Model
{
	public class User
	{
		[Key]
		public Guid Id { get; private set; } = Guid.NewGuid();
		public string EmailorUserName { get; set; }

		public string ?ProfilePicture { get; set; }
		public string ?Password { get; set; }

		public UserRole Role { get; set; }

		public Gender ?gender { get; set; }

		//public bool IsVerified { get; set; } = true;
		public Status status { get; set; } = Status.Active;

		//public bool IsActive => status == Status.Active;

		//public bool IsLockedOut { get; set; } = false;

		public String FName { get; set; } = string.Empty;

		public String LName { get; set; } = string.Empty;

		public string Address { get; set; }

		public string phoneNumber { get; set; }

		//public string TID { get; set; }

		public RefreshToken RefreshToken { get; set; }
		
		
	}
}
