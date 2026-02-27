using System.Net.Mail;
using grad.Application.Common.Interfaces;
using grad.Infrastructure.Persistence.Configurations;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace grad.Infrastructure.EmailServices
{
	public class EmailService : IEmailServices
	{
		private readonly SMTPsettings _settings;

		public EmailService(IOptions<SMTPsettings> options)
		{
			_settings = options.Value;
		}

		private string GenerateHtmlEmail(
	string heading,
	string message,
	string buttonText = "",
	string buttonLink = "")
		{
			return $@"
<!DOCTYPE html>
<html>
  <body style=""margin:0; padding:0; background-color:#f0f7ff; font-family:'Segoe UI', Arial, sans-serif;"">

    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""padding:20px;"">
      <tr>
        <td align=""center"">

          <!-- Card -->
          <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background:#ffffff; border-radius:18px; padding:24px; box-shadow:0 8px 20px rgba(0,0,0,0.08);"">

            <!-- Logo -->
            <tr>
              <td align=""center"" style=""padding-bottom:10px;"">
                <img src=""https://yourdomain.com/logo.png"" width=""110"" alt=""Grad Learning"" />
              </td>
            </tr>

            <!-- Friendly Badge -->
            <tr>
              <td align=""center"" style=""padding-bottom:10px;"">
                <span style=""background:#e8f2ff; color:#4a90e2; padding:6px 14px; border-radius:20px; font-size:13px;"">
                  🌟 Grad Learning System
                </span>
              </td>
            </tr>

            <!-- Heading -->
            <tr>
              <td align=""center"" style=""padding:20px 0 10px;"">
                <h1 style=""margin:0; color:#333; font-size:26px;"">
                  {heading}
                </h1>
              </td>
            </tr>

            <!-- Message -->
            <tr>
              <td style=""color:#555; font-size:16px; line-height:1.6; padding:10px 20px;"">
                {message}
              </td>
            </tr>

            <!-- Button -->
            {(string.IsNullOrEmpty(buttonText) ? "" : $@"
            <tr>
              <td align=""center"" style=""padding:25px 0;"">
                <a href=""{buttonLink}""
                   style=""background:#4a90e2;
                          color:#ffffff;
                          padding:14px 32px;
                          border-radius:30px;
                          text-decoration:none;
                          font-size:16px;
                          font-weight:bold;
                          display:inline-block;"">
                  {buttonText} 🚀
                </a>
              </td>
            </tr>")}

            <!-- Friendly Footer -->
            <tr>
              <td style=""background:#f7fbff; border-radius:12px; padding:16px; font-size:14px; color:#777;"">
                👨‍👩‍👧 This message was sent to a parent or guardian regarding their child's learning journey.<br/>
                If you did not request this, you can safely ignore this email.
              </td>
            </tr>

            <!-- Copyright -->
            <tr>
              <td align=""center"" style=""padding-top:18px; font-size:12px; color:#aaa;"">
                © {DateTime.UtcNow.Year} Grad Learning System • Learning Made Fun 🎨
              </td>
            </tr>

          </table>

        </td>
      </tr>
    </table>

  </body>
</html>";
		}


		private async Task<bool> SendEmailAsync(string toEmail, string subject, string html, CancellationToken ct)
		{
			try
			{
				var email = new MimeMessage();
				email.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
				email.To.Add(MailboxAddress.Parse(toEmail));
				email.Subject = subject;

				email.Body = new BodyBuilder
				{
					HtmlBody = html
				}.ToMessageBody();

				using var client = new MailKit.Net.Smtp.SmtpClient();
				await client.ConnectAsync(_settings.Host, _settings.Port, _settings.EnableSSL, ct);
				await client.AuthenticateAsync(_settings.UserName, _settings.Password, ct);
				await client.SendAsync(email, ct);
				await client.DisconnectAsync(true, ct);

				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Email error: {ex.Message}");
				return false;
			}
		}

		public Task<bool> SendOTPEmail(string toEmail, string otp, CancellationToken cancellationToken = default)
		{
			var html = GenerateHtmlEmail(
				"Your OTP Code",
				$"Your OTP is <strong style='color:#4a90e2'>{otp}</strong>. It is valid for 10 minutes.",
				"Login",
				"https://grad.com/login"
			);

			return SendEmailAsync(toEmail, "Your OTP for Grad Learning", html, cancellationToken);
		}

		public Task<bool> SendPasswordResetEmail(string toEmail, string resetLink, CancellationToken cancellationToken = default)
		{
			var html = GenerateHtmlEmail(
				"Reset Your Password",
				"Click the button below to reset your password. This link is valid for 1 hour.",
				"Reset Password",
				resetLink
			);

			return SendEmailAsync(toEmail, "Password Reset - Grad Learning", html, cancellationToken);
		}

		public Task<bool> SendPasswordResetSuccessEmail(string toEmail, CancellationToken cancellationToken = default)
		{
			var html = GenerateHtmlEmail(
				"Password Reset Successful",
				"Your password has been updated successfully.",
				"Login",
				"https://grad.com/login"
			);

			return SendEmailAsync(toEmail, "Password Reset Successful", html, cancellationToken);
		}

		public Task<bool> SendAccountLockedEmail(string toEmail, string unlockLink, CancellationToken cancellationToken = default)
		{
			var html = GenerateHtmlEmail(
				"Account Locked",
				"Your account has been locked due to multiple failed login attempts.",
				"Unlock Account",
				unlockLink
			);

			return SendEmailAsync(toEmail, "Account Locked - Grad Learning", html, cancellationToken);
		}

		public Task<int> SendConfirmationEmail(string toEmail, string subject)
		{
			throw new NotImplementedException();
		}
	}
}





//using Microsoft.Extensions.Options;
//using Resend;
//using grad.Interfaces;
//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using grad.Data;

//namespace grad.Services
//{
//	public class ResendEmailService : IEmailServices
//	{
//		private readonly IResend _resend;
//		private readonly ResendSettings _settings;

//		public ResendEmailService(IOptions<ResendSettings> options)
//		{
//			_settings = options.Value;
//			_resend = ResendClient.Create(_settings.ApiKey);
//		}

//		private string GenerateHtmlEmail(string heading, string message, string buttonText = "", string buttonLink = "")
//		{
//			return $@"
//<html>
//  <body style=""font-family: Arial, sans-serif; background-color: #f7f9fc; padding: 20px;"">
//    <div style=""max-width: 600px; margin: auto; background-color: #fff; border-radius: 12px; padding: 20px; box-shadow: 0 4px 10px rgba(0,0,0,0.1);"">
//      <!-- Logo -->
//      <div style=""text-align: center; margin-bottom: 20px;"">
//        <img src=""https://yourdomain.com/logo.png"" alt=""Grad Learning"" style=""width:120px; height:auto;""/>
//      </div>

//      <!-- Heading -->
//      <h2 style=""color: #4a90e2; text-align: center;"">{heading}</h2>

//      <!-- Message -->
//      <p style=""font-size: 16px; color: #333; line-height: 1.5;"">
//        {message}
//      </p>

//      <!-- Optional Button -->
//      {(string.IsNullOrEmpty(buttonText) ? "" : $@"<div style=""text-align: center; margin-top: 20px;"">
//        <a href=""{buttonLink}"" style=""background-color: #4a90e2; color: #fff; padding: 12px 24px; border-radius: 8px; text-decoration: none; font-weight: bold;"">{buttonText}</a>
//      </div>")}

//      <p style=""font-size: 12px; color: #999; margin-top: 30px; text-align: center;"">
//        Grad Learning System &copy; {DateTime.UtcNow.Year}. All rights reserved.
//      </p>
//    </div>
//  </body>
//</html>";
//		}

//		// This helper ensures we always have a working "From" for test mode
//		private string GetFromEmail() => string.IsNullOrWhiteSpace(_settings.FromEmail) ? "test@resend.dev" : _settings.FromEmail;

//		public async Task<bool> SendOTPEmail(string toEmail, string otp, CancellationToken cancellationToken = default)
//		{
//			string html = GenerateHtmlEmail(
//				heading: "Your OTP Code",
//				message: $"Hi there! <br/>Your OTP code is: <strong style='color:#4a90e2'>{otp}</strong><br/>It is valid for 10 minutes.",
//				buttonText: "Login",
//				buttonLink: "https://grad.com/login"
//			);

//			var message = new EmailMessage()
//			{
//				From = $"{_settings.FromName} <{GetFromEmail()}>",
//				To = { toEmail },
//				Subject = "Your OTP for Grad Learning",
//				HtmlBody = html
//			};

//			try
//			{
//				var response = await _resend.EmailSendAsync(message, cancellationToken);
//				return response != null && response.Success;
//			}
//			catch (Exception ex)
//			{
//				Console.WriteLine($"Error sending email: {ex.Message}");
//				return false;
//			}
//		}

//		public async Task<bool> SendPasswordResetEmail(string toEmail, string resetLink, CancellationToken cancellationToken = default)
//		{
//			string html = GenerateHtmlEmail(
//				heading: "Reset Your Password",
//				message: $"Hi!<br/>You requested a password reset. Click the button below to set a new password. This link is valid for 1 hour.",
//				buttonText: "Reset Password",
//				buttonLink: resetLink
//			);

//			var message = new EmailMessage()
//			{
//				From = $"{_settings.FromName} <{GetFromEmail()}>",
//				To = { toEmail },
//				Subject = "Password Reset - Grad Learning",
//				HtmlBody = html
//			};

//			try
//			{
//				var response = await _resend.EmailSendAsync(message, cancellationToken);
//				return response != null && response.Success;
//			}
//			catch (Exception ex)
//			{
//				Console.WriteLine($"Error sending email: {ex.Message}");
//				return false;
//			}
//		}

//		public async Task<bool> SendPasswordResetSuccessEmail(string toEmail, CancellationToken cancellationToken = default)
//		{
//			string html = GenerateHtmlEmail(
//				heading: "Password Reset Successful",
//				message: "Hi there! Your Grad Learning account password has been updated successfully. You can now log in with your new password.",
//				buttonText: "Login Now",
//				buttonLink: "https://grad.com/login"
//			);

//			var message = new EmailMessage()
//			{
//				From = $"{_settings.FromName} <{GetFromEmail()}>",
//				To = { toEmail },
//				Subject = "Your Grad Learning Password Was Reset",
//				HtmlBody = html
//			};

//			try
//			{
//				var response = await _resend.EmailSendAsync(message, cancellationToken);
//				return response != null && response.Success;
//			}
//			catch (Exception ex)
//			{
//				Console.WriteLine($"Error sending email: {ex.Message}");
//				return false;
//			}
//		}

//		public async Task<bool> SendAccountLockedEmail(string toEmail, string unlockLink, CancellationToken cancellationToken = default)
//		{
//			string html = GenerateHtmlEmail(
//				heading: "Account Locked",
//				message: $"Your account has been temporarily locked due to multiple failed login attempts. Click the button below to unlock your account.",
//				buttonText: "Unlock Account",
//				buttonLink: unlockLink
//			);

//			var message = new EmailMessage()
//			{
//				From = $"{_settings.FromName} <{GetFromEmail()}>",
//				To = { toEmail },
//				Subject = "Account Locked - Grad Learning",
//				HtmlBody = html
//			};

//			try
//			{
//				var response = await _resend.EmailSendAsync(message, cancellationToken);
//				return response != null && response.Success;
//			}
//			catch (Exception ex)
//			{
//				Console.WriteLine($"Error sending email: {ex.Message}");
//				return false;
//			}
//		}

//		public Task<int> SendConfirmationEmail(string toEmail, string subject)
//		{
//			throw new NotImplementedException();
//		}
//	}
//}






////using System.Net.Http.Headers;
////using System.Text.Json;
////using System.Text;
////using grad.Interfaces;
////using MailerSend.AspNetCore;
////using Microsoft.Extensions.Options;
////using Resend;

////namespace grad.Services
////{
////	public class EmailServices : IEmailServices
////	{
////		//private readonly HttpClient _http;
////		//private readonly MailerSendOptions _options;

////		private readonly IResend _resend;

////		public EmailServices()
////		{
////			var apiKey = Environment.GetEnvironmentVariable("RESEND_API_KEY");
////			_resend = ResendClient.Create(apiKey);
////		}
////		public Task<int> SendConfirmationEmail(string toEmail, string subject)
////		{
////			throw new NotImplementedException();
////		}


////		public async Task<bool> SendOTPEmail(string toEmail, string otp, CancellationToken cancellationToken = default)
////		{
////			try
////			{
////				var message = new EmailMessage()
////				{
////					From = "onboarding@resend.dev",  // use verified domain in production
////					To = { toEmail },
////					Subject = "Your OTP Code",
////					HtmlBody = $"<p>Your OTP code is: <strong>{otp}</strong></p>"
////				};

////				var response = await _resend.EmailSendAsync(message, cancellationToken);
////				return !string.IsNullOrEmpty(response.Content.id);
////			}
////			catch
////			{
////				return false;
////			}
////		}

////		public async Task<bool> SendPasswordResetSuccessEmail(string toEmail, CancellationToken cancellationToken = default)
////		{
////			try
////			{
////				var message = new EmailMessage()
////				{
////					From = "onboarding@resend.dev",
////					To = { toEmail },
////					Subject = "Password Reset Successful",
////					HtmlBody = "<p>Your password has been successfully reset.</p>"
////				};

////				var response = await _resend.EmailSendAsync(message, cancellationToken);
////				return !string.IsNullOrEmpty(response.Content.Id);
////			}
////			catch
////			{
////				return false;
////			}
////		}

////		public async Task<bool> SendAccountLockedEmail(string toEmail, string token, CancellationToken cancellationToken = default)
////		{
////			try
////			{
////				var message = new EmailMessage()
////				{
////					From = "onboarding@resend.dev",
////					To = { toEmail },
////					Subject = "Account Locked",
////					HtmlBody = $"<p>Your account is temporarily locked. Unlock it using this link: <a href='https://yourapp.com/unlock?token={token}'>Unlock</a></p>"
////				};

////				var response = await _resend.EmailSendAsync(message, cancellationToken);
////				return !string.IsNullOrEmpty(response.Content.Id);
////			}
////			catch
////			{
////				return false;
////			}
////		}
////	}

////	//public async Task<bool> SendPasswordResetSuccessEmail(string toEmail, CancellationToken cancellationToken = default)
////	//{
////	//	if (string.IsNullOrWhiteSpace(_options.SenderEmail))
////	//		throw new InvalidOperationException("SenderEmail is not configured in MailerSendOptions.");

////	//	var supportLink = "https://yourdomain.com/support"; // Optional

////	//	var html = $@"
////	//	<html>
////	//	  <body style='font-family: Arial, sans-serif; line-height:1.6; color: #333;'>
////	//		<p>Dear User,</p>

////	//		<h2 style='color:#2e7d32;'>Your Password Has Been Successfully Reset</h2>

////	//		<p>We wanted to let you know that your account password was reset successfully. 
////	//		If this was you, no further action is required.</p>

////	//		<p>If you <strong>did not perform this action</strong>, please reset your password immediately from the
////	//		<a href='https://yourdomain.com/forgot-password' style='color:#007bff;'>Forgot Password</a> page 
////	//		and review your account activity for your security.</p>

////	//		<p style='font-size:0.85rem; color:#777; margin-top:25px;'>
////	//		  If you need assistance, please contact our support team at 
////	//		  <a href='{supportLink}' style='color:#007bff;'>Support</a>.<br/><br/>
////	//		  Stay safe,<br/>
////	//		  Security Team
////	//		</p>
////	//	  </body>
////	//	</html>";

////	//	var text = @"Dear User,
////	//	Your password has been successfully reset.
////	//	If this wasn’t you, please reset it again immediately from the Forgot Password page and review your account.";

////	//	var payload = new
////	//	{
////	//		from = new { email = _options.SenderEmail, name = _options.SenderName },
////	//		to = new[] { new { email = toEmail } },
////	//		subject = "Password Reset Successful",
////	//		text = text,
////	//		html = html
////	//	};

////	//	var json = JsonSerializer.Serialize(payload);
////	//	using var content = new StringContent(json, Encoding.UTF8, "application/json");

////	//	var response = await _http.PostAsync("email", content, cancellationToken);
////	//	var body = await response.Content.ReadAsStringAsync(cancellationToken);

////	//	Console.WriteLine("STATUS: " + response.StatusCode);
////	//	Console.WriteLine("BODY: " + body);

////	//	return response.IsSuccessStatusCode;
////	//}



////	//public async Task<bool> SendAccountLockedEmail(string toEmail, string token, CancellationToken cancellationToken = default)
////	//{
////	//	if (string.IsNullOrWhiteSpace(_options.SenderEmail))
////	//		throw new InvalidOperationException("SenderEmail is not configured in MailerSendOptions.");

////	//	var resetLink = $"https://yourdomain.com/reset-password?token={token}";
////	//	var forgetPasswordLink = "https://yourdomain.com/forgot-password";

////	//	var html = $@"
////	//	<html>
////	//	  <body style='font-family: Arial, sans-serif; line-height:1.6; color: #333;'>
////	//		<h2 style='color:#b00020;'>Account Locked Due to Multiple Failed Attempts</h2>
////	//		<p>We noticed several unsuccessful attempts to sign in to your account. 
////	//		For your security, your account has been <strong>temporarily locked</strong>.</p>

////	//		<p>If this was you and you forgot your password, you can reset it using the link below:</p>

////	//		<p style='margin:20px 0;'>
////	//		  <a href='{resetLink}' 
////	//			 style='background:#007bff; color:#fff; padding:10px 18px; text-decoration:none; border-radius:6px;'>
////	//			Reset Your Password
////	//		  </a>
////	//		</p>

////	//		<p>If the button doesn’t work, copy and paste the link below into your browser:</p>
////	//		<p style='word-break:break-all; font-size:0.9rem; color:#555;'>{resetLink}</p>

////	//		<p style='margin-top:15px;'>
////	//		  Alternatively, you can use our <a href='{forgetPasswordLink}' style='color:#007bff;'>Forgot Password</a> page to securely reset your password.
////	//		</p>

////	//		<p style='font-size:0.85rem; color:#777;'>If this wasn’t you, we recommend changing your password and reviewing your account activity.</p>
////	//		<p style='font-size:0.85rem; color:#777;'>Stay safe,<br/>Security Team</p>
////	//	  </body>
////	//	</html>";

////	//	var text = $@"Your account has been locked due to multiple failed login attempts.
////	//	To reset your password, visit: {resetLink}
////	//	If the link doesn't work, go to the Forgot Password page: {forgetPasswordLink}";

////	//	var payload = new
////	//	{
////	//		from = new { email = _options.SenderEmail, name = _options.SenderName },
////	//		to = new[] { new { email = toEmail } },
////	//		subject = "Account Locked – Action Required",
////	//		text = text,
////	//		html = html
////	//	};

////	//	var json = JsonSerializer.Serialize(payload);
////	//	using var content = new StringContent(json, Encoding.UTF8, "application/json");

////	//	var response = await _http.PostAsync("email", content, cancellationToken);
////	//	var body = await response.Content.ReadAsStringAsync(cancellationToken);

////	//	Console.WriteLine("STATUS: " + response.StatusCode);
////	//	Console.WriteLine("BODY: " + body);

////	//	return response.IsSuccessStatusCode;
////	//}






////	//public async Task<bool> SendOTPEmail(string toEmail, string OTP, CancellationToken cancellationToken = default)
////	//{
////	//	if (string.IsNullOrWhiteSpace(_options.SenderEmail))
////	//		throw new InvalidOperationException("SenderEmail is not configured in MailerSendOptions.");

////	//	var expiry = 10;
////	//	var html = $@"
////	//	<html>
////	//	  <body style='font-family: Arial, sans-serif; line-height:1.4;'>
////	//		<h2 style='color:#333'>Your verification code</h2>
////	//		<p>Use the code below to confirm your action. This code <strong>expires in {expiry} minutes</strong>.</p>
////	//		<div style='margin:15px 0; padding:10px; background:#f7f7f7; display:inline-block; border-radius:6px; font-size:1.2rem; letter-spacing:4px;'>
////	//		  <strong>{OTP}</strong>
////	//		</div>
////	//		<p style='color:#666; font-size:0.9rem'>If you did not request this, please ignore this email.</p>
////	//	  </body>
////	//	</html>";

////	//	var text = $"Your verification code: {OTP}\nExpires in {expiry} minutes.";

////	//	var payload = new
////	//	{
////	//		from = new { email = _options.SenderEmail, name = _options.SenderName },
////	//		to = new[] { new { email = toEmail } },
////	//		subject = "Your verification code",
////	//		text = text,
////	//		html = html
////	//	};

////	//	var json = JsonSerializer.Serialize(payload);
////	//	using var content = new StringContent(json, Encoding.UTF8, "application/json");

////	//	var response = await _http.PostAsync("email", content, cancellationToken);
////	//	var body = await response.Content.ReadAsStringAsync(cancellationToken);

////	//	Console.WriteLine("STATUS: " + response.StatusCode);
////	//	Console.WriteLine("BODY: " + body);

////	//	return response.IsSuccessStatusCode;
////	//}


////}
////}
