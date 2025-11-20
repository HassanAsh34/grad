using grad.Model;

namespace grad.Extensions
{
	public static class RolesExtention
	{
		public static string ToString(this User.UserRole role)
		{
			return role switch
			{
				User.UserRole.Admin => "Admin",
				User.UserRole.Student => "Student",
				User.UserRole.Parent => "Parent",
				User.UserRole.Teacher => "Teacher",
				_ => "Unknown"
			};
		}
	}
}
