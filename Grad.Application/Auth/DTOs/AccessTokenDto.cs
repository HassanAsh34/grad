namespace Grad.Application.Auth.DTOs
{
	public class AccessTokenDto
	{
		public Guid ID { get; set; }

		public string EmailorUserName { get; set; }

		public string Role { get; set; }

		public bool expired { get; set; }

		public TimeSpan remainingtime { get; set; }

		public string Token { get; set; }
	}
}
