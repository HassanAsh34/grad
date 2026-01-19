using System.Threading;
using grad.DTO;
using grad.Model;

namespace grad.Interfaces
{
	public interface IUserServices
	{
		//public string GenerateToken(User user);

		public Task<ResultDTO> login(LoginDTO login, CancellationToken cancellationToken = default);

		//public Task<object> register(SignupDTO user, CancellationToken cancellationToken);

		public Task<ResultDTO> register(SignupDTO? user = null, Student? student = null, bool Reg = false, CancellationToken cancellationToken = default);
		public Task<ResultDTO> RequestChangePass(User user,CancellationToken cancellationToken = default);

		public Task<ResultDTO> ResetPasswordOTP(OTP_DTO otp, CancellationToken cancellationToken = default);

		//public Task<ResultDTO> refreshToken(string token, CancellationToken cancellationToken = default);

		public Task<ResultDTO> refreshToken(ResponseTokenDTO token, CancellationToken cancellationToken = default);


		//public Task<ResultDTO> ResetPassword(string token, string NewPassword, User user);

		//public Task<ResultDTO> ResetPassword(string password, User user = null);
		//public Task<ResultDTO> ResetPassword(ChangePasswordDTO changePassword, CancellationToken cancellationToken = default);

		public Task<ResultDTO> ResetPassword(ChangePasswordDTO changePassword, bool resetToken = false, CancellationToken cancellationToken = default);

		public Task<ResultDTO> ViewProfile(ProfileDTO user, CancellationToken cancellationToken = default);

		//public Task<ResultDTO> LogOut(string token, string uid, CancellationToken cancellationToken = default);
		public Task<ResultDTO> LogOut(string token, string refreshtoken, string uid, CancellationToken cancellationToken = default);



		//string generateToken(User user);

		//public Task<object> DeleteAccount();

		//public Task<object> updateProfile(User user);



	}
}
