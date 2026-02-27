using System.Text.Json.Serialization;

namespace grad.Application.Auth.DTOs
{
	public class RequestRefreshToken
	{
		[JsonIgnore]
		public Guid? uid { get; set; }

		[JsonIgnore]
		public string? AccessToken { get; set; }
		public string RefreshToken { get; set; }
	}
}
