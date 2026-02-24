namespace Grad_Structured.Application.Common.Interfaces
{
	public interface IEmailServices
	{
		public Task<bool>  SendOTPEmail(string toEmail,string OTP, CancellationToken cancellationToken = default);

		public Task<bool> SendAccountLockedEmail(string toEmail, string token, CancellationToken cancellationToken = default);

		public Task<int> SendConfirmationEmail(string toEmail, string subject);

		public Task<bool> SendPasswordResetSuccessEmail(string toEmail, CancellationToken cancellationToken = default);
	}
}
