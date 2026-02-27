namespace grad.Application.Auth.DTOs
{
	public class RefreshTokenDTO
	{
		public Guid Id { get; set; }

		public Guid Uid { get; set; }

		public int role { get; set; }

		public bool expired { get; set; }

		public TimeSpan remainingtime { get; set; }

		public string TokenKey { get; set; }
	}
}
