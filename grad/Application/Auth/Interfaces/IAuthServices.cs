
using Grad_Structured.Domain.Model;
using Grad_Structured.Application.Auth.DTOs;
using Grad_Structured.Application.Common.DTOs;
using Grad_Structured.Application.parent.DTOs;



namespace Grad_Structured.Application.Auth.Interfaces
{
	public interface IAuthServices
	{
		public Task<ResultDTO> login(LoginDTO login, CancellationToken cancellationToken = default);
		public Task<ResultDTO> register(SignupDTO? user = null, RegisterStudentDTO? student = null, bool Reg = false, CancellationToken cancellationToken = default);
		public Task<ResultDTO> RequestChangePass(User user, CancellationToken cancellationToken = default);
		public Task<ResultDTO> ResetPasswordOTP(OTP_DTO otp, CancellationToken cancellationToken = default);
		public Task<ResultDTO> refreshToken(string token, CancellationToken cancellationToken = default);
		public Task<ResultDTO> ResetPassword(ChangePasswordDTO changePassword, bool resetToken = false, CancellationToken cancellationToken = default);
		public Task<ResultDTO> LogOut(LogoutDTO login, CancellationToken cancellationToken = default);
		public Task<bool> UserExists(string? EmailorUserName = "", Guid? uid = null, CancellationToken cancellationToken = default);
	}
}
