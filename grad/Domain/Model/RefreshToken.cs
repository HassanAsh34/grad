using System.ComponentModel.DataAnnotations;

namespace grad.Domain.Model
{
	public class RefreshToken
	{
		[Key]
		public Guid Id { get; private set; } = Guid.NewGuid();

		public string TokenKey { get; set; }

		//public DateTime Expires { get; set; } = DateTime.UtcNow.AddDays(7); //long lived

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public Guid CreatedById { get; set; }

		public User User { get; set; }

		public bool Revoked { get; set; }

		//public string RevokedByIp { get; set; }

		//public string ReplacedByToken { get; set; }

		public bool IsActive => !Revoked;

		//public bool IsExpired => DateTime.UtcNow >= Expires;

	}
}
