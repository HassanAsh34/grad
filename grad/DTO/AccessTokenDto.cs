using Google.Cloud.Firestore;

namespace grad.DTO
{
	public class AccessTokenDto
	{
		public string ID { get; set; }

		public string EmailorUserName { get; set; }

		public string Role { get; set; }

		public bool expired { get; set; }

		public TimeSpan remainingtime { get; set; }

		public string Token { get; set; }
	}
}
