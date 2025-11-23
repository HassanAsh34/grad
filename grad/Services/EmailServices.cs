using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using grad.Interfaces;
using MailerSend.AspNetCore;
using Microsoft.Extensions.Options;

namespace grad.Services
{
	public class EmailServices : IEmailServices
	{
		private readonly HttpClient _http;
		private readonly MailerSendOptions _options;

		public EmailServices(HttpClient http, IOptions<MailerSendOptions> opt)
		{
			_http = http;
			_options = opt.Value;
			if (_http.BaseAddress == null)
				_http.BaseAddress = new Uri("https://api.mailersend.com/v1/");
			if (!string.IsNullOrWhiteSpace(_options.ApiToken))
				_http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiToken);
		}
		public Task<int> SendConfirmationEmail(string toEmail, string subject)
		{
			throw new NotImplementedException();
		}

		public async Task<bool> SendPasswordResetSuccessEmail(string toEmail, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(_options.SenderEmail))
				throw new InvalidOperationException("SenderEmail is not configured in MailerSendOptions.");

			var supportLink = "https://yourdomain.com/support"; // Optional

			var html = $@"
			<html>
			  <body style='font-family: Arial, sans-serif; line-height:1.6; color: #333;'>
				<p>Dear User,</p>

				<h2 style='color:#2e7d32;'>Your Password Has Been Successfully Reset</h2>

				<p>We wanted to let you know that your account password was reset successfully. 
				If this was you, no further action is required.</p>

				<p>If you <strong>did not perform this action</strong>, please reset your password immediately from the
				<a href='https://yourdomain.com/forgot-password' style='color:#007bff;'>Forgot Password</a> page 
				and review your account activity for your security.</p>

				<p style='font-size:0.85rem; color:#777; margin-top:25px;'>
				  If you need assistance, please contact our support team at 
				  <a href='{supportLink}' style='color:#007bff;'>Support</a>.<br/><br/>
				  Stay safe,<br/>
				  Security Team
				</p>
			  </body>
			</html>";

			var text = @"Dear User,
			Your password has been successfully reset.
			If this wasn’t you, please reset it again immediately from the Forgot Password page and review your account.";

			var payload = new
			{
				from = new { email = _options.SenderEmail, name = _options.SenderName },
				to = new[] { new { email = toEmail } },
				subject = "Password Reset Successful",
				text = text,
				html = html
			};

			var json = JsonSerializer.Serialize(payload);
			using var content = new StringContent(json, Encoding.UTF8, "application/json");

			var response = await _http.PostAsync("email", content, cancellationToken);
			var body = await response.Content.ReadAsStringAsync(cancellationToken);

			Console.WriteLine("STATUS: " + response.StatusCode);
			Console.WriteLine("BODY: " + body);

			return response.IsSuccessStatusCode;
		}



		public async Task<bool> SendAccountLockedEmail(string toEmail, string token, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(_options.SenderEmail))
				throw new InvalidOperationException("SenderEmail is not configured in MailerSendOptions.");

			var resetLink = $"https://yourdomain.com/reset-password?token={token}";
			var forgetPasswordLink = "https://yourdomain.com/forgot-password";

			var html = $@"
			<html>
			  <body style='font-family: Arial, sans-serif; line-height:1.6; color: #333;'>
				<h2 style='color:#b00020;'>Account Locked Due to Multiple Failed Attempts</h2>
				<p>We noticed several unsuccessful attempts to sign in to your account. 
				For your security, your account has been <strong>temporarily locked</strong>.</p>

				<p>If this was you and you forgot your password, you can reset it using the link below:</p>

				<p style='margin:20px 0;'>
				  <a href='{resetLink}' 
					 style='background:#007bff; color:#fff; padding:10px 18px; text-decoration:none; border-radius:6px;'>
					Reset Your Password
				  </a>
				</p>

				<p>If the button doesn’t work, copy and paste the link below into your browser:</p>
				<p style='word-break:break-all; font-size:0.9rem; color:#555;'>{resetLink}</p>

				<p style='margin-top:15px;'>
				  Alternatively, you can use our <a href='{forgetPasswordLink}' style='color:#007bff;'>Forgot Password</a> page to securely reset your password.
				</p>

				<p style='font-size:0.85rem; color:#777;'>If this wasn’t you, we recommend changing your password and reviewing your account activity.</p>
				<p style='font-size:0.85rem; color:#777;'>Stay safe,<br/>Security Team</p>
			  </body>
			</html>";
				
			var text = $@"Your account has been locked due to multiple failed login attempts.
			To reset your password, visit: {resetLink}
			If the link doesn't work, go to the Forgot Password page: {forgetPasswordLink}";

			var payload = new
			{
				from = new { email = _options.SenderEmail, name = _options.SenderName },
				to = new[] { new { email = toEmail } },
				subject = "Account Locked – Action Required",
				text = text,
				html = html
			};

			var json = JsonSerializer.Serialize(payload);
			using var content = new StringContent(json, Encoding.UTF8, "application/json");

			var response = await _http.PostAsync("email", content, cancellationToken);
			var body = await response.Content.ReadAsStringAsync(cancellationToken);

			Console.WriteLine("STATUS: " + response.StatusCode);
			Console.WriteLine("BODY: " + body);

			return response.IsSuccessStatusCode;
		}






		public async Task<bool> SendOTPEmail(string toEmail, string OTP, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(_options.SenderEmail))
				throw new InvalidOperationException("SenderEmail is not configured in MailerSendOptions.");

			var expiry = 10;
			var html = $@"
			<html>
			  <body style='font-family: Arial, sans-serif; line-height:1.4;'>
				<h2 style='color:#333'>Your verification code</h2>
				<p>Use the code below to confirm your action. This code <strong>expires in {expiry} minutes</strong>.</p>
				<div style='margin:15px 0; padding:10px; background:#f7f7f7; display:inline-block; border-radius:6px; font-size:1.2rem; letter-spacing:4px;'>
				  <strong>{OTP}</strong>
				</div>
				<p style='color:#666; font-size:0.9rem'>If you did not request this, please ignore this email.</p>
			  </body>
			</html>";

			var text = $"Your verification code: {OTP}\nExpires in {expiry} minutes.";

			var payload = new
			{
				from = new { email = _options.SenderEmail, name = _options.SenderName },
				to = new[] { new { email = toEmail } },
				subject = "Your verification code",
				text = text,
				html = html
			};

			var json = JsonSerializer.Serialize(payload);
			using var content = new StringContent(json, Encoding.UTF8, "application/json");

			var response = await _http.PostAsync("email", content, cancellationToken);
			var body = await response.Content.ReadAsStringAsync(cancellationToken);

			Console.WriteLine("STATUS: " + response.StatusCode);
			Console.WriteLine("BODY: " + body);

			return response.IsSuccessStatusCode;
		}


	}
}
