namespace grad.Model
{
	public class RefreshToken
	{
		public int Id { get; set; }

		public string Token { get; set; }

		public DateTime Expires { get; set; } = DateTime.UtcNow.AddDays(7); //long lived

		public DateTime Created { get; set; } = DateTime.UtcNow;

		public string CreatedById { get; set; }

		public User User { get; set; }

		public bool Revoked { get; set; }

		//public string RevokedByIp { get; set; }

		//public string ReplacedByToken { get; set; }

		public bool IsActive => !Revoked && !IsExpired;

		public bool IsExpired => DateTime.UtcNow >= Expires;

	}
}
