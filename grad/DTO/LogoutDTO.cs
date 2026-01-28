namespace grad.DTO
{
	public class LogoutDTO
	{
		public string RefreshToken { get; set; }

		public string accessToken { get; set; }

		public int RemainingTimeAcc { get; set; }
	}
}
