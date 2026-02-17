using grad.Interfaces;
using grad.Model;
using static grad.Model.User;

public static class DataSeeder
{
	public static async Task SeedAdminAsync(
		IRepository repository,
		IUowServices uow)
	{
		var adminEmail = "Admin@System.com";

		var adminExists = await repository.GetEntityAsync<User>(
			u => u.Role == UserRole.Admin &&
				 u.EmailorUserName == adminEmail);

		if (adminExists != null)
			return; // ✅ already exists → do nothing

		var admin = new Admin
		{
			AdminType = "SuperAdmin",
			EmailorUserName = adminEmail,
			status = Status.Active,
			Role = UserRole.Admin,
			Address = "",
			phoneNumber= "01553244141",
			Password = BCrypt.Net.BCrypt.HashPassword("Admin@123")
		};

		repository.CreateEntityAsync<Admin>(admin);
		await uow.SaveChangesAsync();
	}
}
