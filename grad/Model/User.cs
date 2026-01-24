using System.ComponentModel.DataAnnotations;

namespace grad.Model
{
	public class User
	{
		[Key]
		public string Id { get; private set; } = Guid.NewGuid().ToString();
		public string EmailorUserName { get; set; }

		public string ?ProfilePicture { get; set; }
		public string Password { get; set; }

		public UserRole Role { get; set; }

		public Gender ?gender { get; set; }

		public bool IsVerified { get; set; } = true;

		public bool IsActive { get; set; } = true;

		public bool IsLockedOut { get; set; } = false;


		//public string TID { get; set; }

		public RefreshToken RefreshToken { get; set; }
		public enum UserRole
		{
			Admin,
			Student,
			Parent,
			Teacher
		}

		public enum Gender
		{
			Male = 1,
			Female = 2
		}
	}
}
