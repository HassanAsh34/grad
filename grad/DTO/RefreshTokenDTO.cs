namespace grad.DTO
{
	public class RefreshTokenDTO
	{
		public string Id { get; set; }

		public string Uid { get; set; }

		public int role { get; set; }

		public bool expired { get; set; }

		public TimeSpan remainingtime { get; set; }

		public string TokenKey { get; set; }
	}
}
