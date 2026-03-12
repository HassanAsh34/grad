using System.Security.Cryptography;
using Grad.Application.Auth.DTOs;
using Grad.Application.Auth.Interfaces;
using Grad.Application.Common.DTOs;
using Grad.Application.Common.Interfaces;
using Grad.Application.ParentFeatures.DTOs;
using Grad.Domain.Enums;
using Grad.Domain.Model;
using Microsoft.AspNetCore.Http;

namespace Grad.Application.Auth.Services
{
	public class AuthServices : IAuthServices
	{
		private readonly IAuthRepository _repository;
		private readonly IRedisServices _redis;
		private readonly IEmailServices _emailServices;
		private readonly ITokenServices _TokenServices;
		private readonly IUowServices _uow;
		private readonly ICloudinaryServices _cloudinaryServices;

		public AuthServices(IAuthRepository authRepository, IRedisServices redis, IEmailServices emailServices, ITokenServices tokenServices, IUowServices uow, ICloudinaryServices cloudinaryServices)
		{
			_repository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
			_redis = redis ?? throw new ArgumentNullException(nameof(redis));
			_emailServices = emailServices ?? throw new ArgumentNullException(nameof(emailServices));
			_TokenServices = tokenServices ?? throw new ArgumentNullException(nameof(tokenServices));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_cloudinaryServices = cloudinaryServices ?? throw new ArgumentNullException(nameof(cloudinaryServices));
		}

		public async Task<ResultDTO> register(SignupDTO? user, RegisterStudentDTO? studentDTO, bool Reg, CancellationToken cancellationToken)
		{
			User u = null;
			IFormFile file = null;
			if (Reg)
			{
				if (studentDTO == null)
				{
					return new ResultDTO
					{
						StatusCode = 400,
						Message = "Invalid student data"
					};
				}
				else
				{
					Parent parent = await _repository.GetEntityAsync<Parent>(p => p.Id == studentDTO.p_Id, cancellationToken: cancellationToken);
					if (parent == null)
						return new ResultDTO
						{
							StatusCode = 401,
							Message = "Something went wrong please try again later"
						};
					Student student = new Student
					{
						FName = studentDTO.FName,
						LName = studentDTO.Pname.Split(' ')[0],
						age = DateOnly.FromDateTime(DateTime.UtcNow).Year - studentDTO.BirthDate.Year,
						EmailorUserName = studentDTO.Email,
						PID = parent.Id,
						BirthDate = studentDTO.BirthDate,
						Address = parent.Address,
						phoneNumber = parent.phoneNumber,
						Disability = (DisablityType)studentDTO.Disability,
						gender = studentDTO.gender == 1 ? Gender.Male : studentDTO.gender == 2 ? Gender.Female : 0,
						Role = UserRole.Student,
						Password = studentDTO.password
					};
					_repository.CreateEntityAsync<Student>(student, cancellationToken: cancellationToken);
					u = student;
					file = studentDTO.image;
				}
			}
			else
			{
				if (await UserExists(EmailorUserName: user.email, cancellationToken: cancellationToken))
				{
					return new ResultDTO
					{
						StatusCode = 409,
						Message = "User already exists"
					};
				}
				user.password = BCrypt.Net.BCrypt.HashPassword(user.password);
				object result;
				switch (user.role)
				{
					//case (int)UserRole.Admin:
					//	Admin admin = new Admin
					//	{
					//		EmailorUserName = user.email,
					//		Password = user.password,
					//		Role = User.UserRole.Admin,

					//	};
					//	_repository.CreateEntityAsync<Admin>(admin, cancellationToken: cancellationToken);
					//	u = admin;
					//	break;
					case (int)UserRole.Parent:
						Parent parent = new Parent
						{
							EmailorUserName = user.email,
							Password = user.password,
							FName = user.FName,
							LName = user.LName,
							phoneNumber = user.phoneNumber,
							Address = user.Address,
							BirthDate = user.BD,
							Job = user.Job,
							gender = user.Gender == 1 ? Gender.Male : user.Gender == 2 ? Gender.Female : 0,
							Role = UserRole.Parent
						};
						_repository.CreateEntityAsync<Parent>(parent, cancellationToken: cancellationToken);
						u = parent;
						break;
					case (int)UserRole.Teacher:
						Teacher teacher = new Teacher
						{
							EmailorUserName = user.email,
							Password = user.password,
							FName = user.FName,
							LName = user.LName,
							phoneNumber = user.phoneNumber,
							Address = user.Address,
							BirthDate = user.BD,
							Role = UserRole.Teacher,
							gender = user.Gender == 1 ? Gender.Male : user.Gender == 2 ? Gender.Female : 0,
							status = Status.Pending
						};
						_repository.CreateEntityAsync<Teacher>(teacher, cancellationToken: cancellationToken);
						u = teacher;
						break;
					//case student need to be implemented
					case (int)UserRole.Student:
						Student student = new Student
						{
							FName = user.FName,
							LName = user.LName,
							age = GetYearsDifference(user.BD, DateOnly.FromDateTime(DateTime.UtcNow)),
							EmailorUserName = user.email,
							phoneNumber = user.phoneNumber,
							Address = user.Address,
							//PID = studentDTO.p_Id,
							BirthDate = user.BD,
							Disability = (DisablityType)(user.Disability == null ? 0 : user.Disability),
							gender = user.Gender == 1 ? Gender.Male : user.Gender == 2 ?	Gender.Female : 0,
							Role = UserRole.Student,
							Password = user.password
						};
						if (student.age == 0)
						{
							return new ResultDTO
							{
								StatusCode = 400,
								Message = "Invalid birth date"
							};
						}
						_repository.CreateEntityAsync<Student>(student, cancellationToken: cancellationToken);
						u = student;
						break;
					default:
						return new ResultDTO
						{
							StatusCode = 400,
							Message = "Registration Failed"
						};
				}
				file = user.file;
			}
			int res = await _uow.SaveChangesAsync();
			if (res == 0)
				return new ResultDTO
				{
					StatusCode = 400,
					Message = "Registration Failed"
				};
			else
			{
				if (file != null)
					await storeImage(u,file, cancellationToken);
				return new ResultDTO
				{
					StatusCode = 201,
					Message = "Thanks for working with us"
				};
			}
		}

		private async Task storeImage(User user,IFormFile file, CancellationToken cancellationToken = default) // implement cloudinary
		{
			if (file != null && file.Length > 0)
			{
				//string userFolder = Path.Combine("uploads", "users");
				//if (!System.IO.File.Exists(userFolder))
				//{
				//	Directory.CreateDirectory(userFolder);
				//}

				//user.ProfilePicture = Path.Combine(userFolder, $"{user.Id}.jpg");

				//using var stream = new FileStream(user.ProfilePicture, FileMode.Create);
				//await file.CopyToAsync(stream, cancellationToken);
				string publicId = $"{user.Id}";
				user.ProfilePicture = await _cloudinaryServices.UploadImageAsync(file,"user", publicId, cancellationToken);
				_repository.UpdateEntityAsync<User>(user, cancellationToken: cancellationToken);
				await _uow.SaveChangesAsync();
			}
		}

		public async Task<ResultDTO> login(LoginDTO login, CancellationToken cancellationToken) //not complete yet
		{
			User user = new User
			{
				EmailorUserName = login.UsernameorEmail.ToLower().Trim(),
			};
			user = await _repository.GetEntityAsync<User>(u => u.EmailorUserName.ToLower().Equals(user.EmailorUserName), cancellationToken: cancellationToken);
			if (user == null)
			{
				return new ResultDTO
				{
					StatusCode = 400,
					Message = "invalid credentials"
				};
			}
			else
			{
				switch (user.status)
				{
					case Status.Banned:
						return new ResultDTO
						{
							StatusCode = 403,
							Message = "account is banned"
						};
					case Status.Pending:
						return new ResultDTO
						{
							StatusCode = 403,
							Message = "account is pending approval"
						};
					case Status.Inactive:
						return new ResultDTO
						{
							StatusCode = 403,
							Message = "account is deactivated"
						};
					case Status.locked:
						return new ResultDTO
						{
							StatusCode = 403,
							Message = "Your account is temporarily locked after several unsuccessful login attempts. You can regain access by resetting your password."
						};
					case Status.Active:
						break;
				}
				//if (!user.IsActive)
				//{
				//	return new ResultDTO
				//	{
				//		StatusCode = 403,
				//		Message = "account is deactivated"
				//	};
				//}
				//else if (!user.IsVerified)
				//{
				//	return new ResultDTO
				//	{
				//		StatusCode = 403,
				//		Message = "account is not verified"
				//	};
				//}
				//else if (user.IsLockedOut)
				//{
				//	return new ResultDTO
				//	{
				//		StatusCode = 403,
				//		Message = "Your account is temporarily locked after several unsuccessful login attempts. You can regain access by resetting your password."
				//	};
				//}
				if (await getAttempts(user.EmailorUserName) >= 5) //dont forget to impletement refresh token part
				{
					user.status = Status.locked;
					_repository.UpdateEntityAsync<User>(user);
					await _uow.SaveChangesAsync();
					string token = _TokenServices.generateAccessToken(user, true);
					//need to configure link to unlock account
					await _emailServices.SendAccountLockedEmail(user.EmailorUserName, "");//TODO: add unlock link 
					return new ResultDTO
					{
						StatusCode = 400,
						Message = "invalid credentials"
					};
				}
				else
				{
					bool verified = BCrypt.Net.BCrypt.Verify(login.password, user.Password);
					if (!verified)
					{
						await storeAttempts(user.EmailorUserName);
						//await _repository.UpdateEntityAsync<User>(user);
						return new ResultDTO
						{
							StatusCode = 400,
							Message = "invalid credentials"
						};
					}
					else
					{
						await storeAttempts(user.EmailorUserName, true);

						return await getTokenAsync(user, cancellationToken: cancellationToken);
					}
				}
			}
		}



		public async Task<ResultDTO> RequestChangePass(string email, CancellationToken cancellationToken)
		{

			string Email = await _repository.getEmail(email, cancellationToken);
			if (string.IsNullOrEmpty(Email))
				return new ResultDTO
				{
					StatusCode = 200,
					Message = "If the email exists, an OTP has been sent."
				};
			int otpInt = RandomNumberGenerator.GetInt32(100000, 1000000);
			string otp = otpInt.ToString();
			string hashedotp = BCrypt.Net.BCrypt.HashPassword(otp);
			bool res = await storeOTP(email, hashedotp);
			if (res)
			{
				Console.WriteLine($"otp {otp}");
				//
				if (await _emailServices.SendOTPEmail(Email, otp, cancellationToken))
					return new ResultDTO
					{
						StatusCode = 200,
						Message = "If the email exists, an OTP has been sent."
					};
				else
				{
					return new ResultDTO
					{
						StatusCode = 500,
						Message = "Error sending OTP"
					};
				}
			}
			else
			{
				return new ResultDTO
				{
					StatusCode = 500,
					Message = "Error sending OTP"
				};
			}
		}

		public async Task<ResultDTO> ResetPasswordOTP(OTP_DTO otp, CancellationToken cancellationToken)
		{
			if (otp == null)
				return new ResultDTO
				{
					StatusCode = 400,
					Message = "Invalid OTP data"
				};
			else
			{
				var res = await getOTP(otp.EmailorUserName);
				if (res == null)
					return new ResultDTO
					{
						StatusCode = 400,
						Message = "Invalid OTP"
					};
				else
				{
					bool verify = BCrypt.Net.BCrypt.Verify(otp.OTP, res);
					if (verify)
					{
						var user = await _repository.GetEntityAsync<User>(u => u.EmailorUserName.ToLower().Equals(otp.EmailorUserName.ToLower()), cancellationToken: cancellationToken);
						if (user == null)
						{
							return new ResultDTO
							{
								StatusCode = 500,
								Message = "Something went wrong"
							};
						}
						//else if(BCrypt.Net.BCrypt.Verify(otp.NewPassword,user.Password))
						//{
						//	return new ResultDTO
						//	{
						//		StatusCode = 400,
						//		Message = "New password cannot be the same as the old password"
						//	};
						//}
						await deleteOTP(otp.EmailorUserName);
						//return await savePassword(user,otp.,cancellationToken: cancellationToken);
						return new ResultDTO
						{
							Message = "Verified",
							StatusCode = 200,
							result = _TokenServices.generateAccessToken(user, true)
							//result = new
							//{
							//	AccessToken =  _TokenServices.generateAccessToken(user,true) //dont forget to change the returned token to be stored directly into into cookies
							//}
						}
					;
					}
					else
					{
						return new ResultDTO
						{
							StatusCode = 400,
							Message = "Invalid OTP"
						};
					}
				}
			}
		}



		public async Task<ResultDTO> ResetPassword(ChangePasswordDTO changePassword, bool resetToken, CancellationToken cancellationToken) /// we need to reconfigure the whole function?
		{
			
			//User user = null;
			User user = await _repository.getUserWithRefreshToken(changePassword.Id, cancellationToken);
			if (user == null)
				return new ResultDTO
				{
					Message = "something went wrong",
					StatusCode = 403
				};
			else
			{
				if (user.RefreshToken != null)
				{
					user.RefreshToken.Revoked = true;
				}
				//if (!changePassword.token.IsNullOrEmpty())
				//{
				//	await _TokenServices.blacklistToken(changePassword.token);
				//}
				if (!resetToken)
				{
					if (!string.IsNullOrEmpty(changePassword.OldPassword))
					{
						bool verified = BCrypt.Net.BCrypt.Verify(changePassword.OldPassword, user.Password);
						if (!verified)
						{
							return new ResultDTO
							{
								StatusCode = 400,
								Message = "Wrong password"
							};
						}
						else
						{
							if (changePassword.OldPassword.ToLower().Equals(changePassword.NewPassword.ToLower()))
								return new ResultDTO
								{
									StatusCode = 400,
									Message = "New password should not match the old password"
								};
						}
					}
					else
						return new ResultDTO
						{
							StatusCode = 400,
							Message = "Old password cant be empty"
						};
				}
				return await savePassword(user, changePassword.NewPassword, changePassword.token, cancellationToken: cancellationToken);
			}
		}

		public async Task<ResultDTO> LogOut(LogoutDTO logout, CancellationToken cancellationToken = default)
		{
			if (logout == null)
			{
				return new ResultDTO { StatusCode = 400, Message = "Invalid logout request" };
			}

			// 1. Attempt to revoke the refresh token (if provided)
			if (!string.IsNullOrEmpty(logout.RefreshToken))
			{
				// We don't fail the logout if this returns false. 
				// The token might already be revoked or expired.
				await _TokenServices.RevokeRefreshToken(logout.RefreshToken, cancellationToken);
			}

			// 2. Attempt to blacklist the access token (if provided)
			if (!string.IsNullOrEmpty(logout.accessToken))
			{
				int remaining = logout.RemainingTimeAcc > 0 ? logout.RemainingTimeAcc : 30;
				// We do not strictly fail if blacklistToken returns false, because 
				// Redis might be disabled locally via configuration.
				await _TokenServices.blacklistToken(logout.accessToken, remaining);
			}

			// Always return success to the client so it can clear its local state
			return new ResultDTO
			{
				Message = "Logged Out",
				StatusCode = 200
			};
		}

		public async Task<bool> UserExists(string? EmailorUserName = "", Guid? uid = null, CancellationToken cancellationToken = default)
		{
			return await _repository.IsUserExist(EmailorUserName, uid, cancellationToken);
		}


		private async Task<ResultDTO> savePassword(User user, string NewPassword, string token, CancellationToken cancellationToken = default)// we need to add the token to black list 
		{
			//revoke user token
			//bool res = await revokeToken(user.RefreshToken, cancellationToken);
			revokeToken(user.RefreshToken, cancellationToken);
			if (user.status == Status.locked)
				user.status = Status.Active;
			user.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword);
			_repository.UpdateEntityAsync<User>(user, cancellationToken: cancellationToken);
			int updateRes = await _uow.SaveChangesAsync();
			if (updateRes != 0)
			{
				await _emailServices.SendPasswordResetSuccessEmail(user.EmailorUserName);
				// remove comment
				await _TokenServices.blacklistToken(token);
				return new ResultDTO
				{
					StatusCode = 200,
					Message = "Password changed successfully"
				};
			}
			else
			{
				return new ResultDTO
				{
					StatusCode = 500,
					Message = "Failed to update password"
				};
			}
		}

		private void revokeToken(RefreshToken token, CancellationToken cancellationToken)
		{
			if (token != null)
			{
				token.Revoked = true;
				_repository.UpdateEntityAsync(token, cancellationToken: cancellationToken);
				//return await _repository.UpdateEntityAsync<RefreshToken>(token, cancellationToken);
			}
		}
		private async Task<bool> storeOTP(string emailorUserName, string hashedotp)
		{
			return await _redis.store(emailorUserName.ToLower(), hashedotp, TimeSpan.FromMinutes(10));
		}

		private async Task<string> getOTP(string emailorUserName)
		{
			return await _redis.get(emailorUserName.ToLower());
		}

		private async Task<bool> deleteOTP(string emailorUserName)
		{
			return await _redis.delete(emailorUserName.ToLower());
		}

		private async Task<bool> storeAttempts(string emailorUserName, bool verified = false)
		{
			string key = $"{emailorUserName}_attempts".ToLower();
			if (verified)
			{
				return await _redis.delete(key);
			}
			int attempts = await getAttempts(emailorUserName) + 1;
			return await _redis.store(key, attempts.ToString(), TimeSpan.FromMinutes(30));
		}
		private async Task<int> getAttempts(string emailorUserName)
		{
			string key = $"{emailorUserName}_attempts".ToLower();
			if (int.TryParse(await _redis.get(key), out int result))
			{
				return result;
			}
			return 0;
		}

		private async Task<ResultDTO> getTokenAsync(User user = null, bool refresh = false, string refreshToken = "", CancellationToken cancellationToken = default)
		{
			if (refresh)
			{
				if (string.IsNullOrEmpty(refreshToken))
				{
					return new ResultDTO
					{
						Message = "Something Went Wrong please try to login again",
						StatusCode = 401
					};
				}
				else
				{
					RefreshTokenDTO refreshTokenDTO = await _TokenServices.getTokenInfo(refreshToken, refresh, cancellationToken);
					if (refreshTokenDTO != null)
					{
						User u = await _repository.GetEntityAsync<User>(usr => usr.Id == refreshTokenDTO.Uid, cancellationToken: cancellationToken);
						if (u != null)
						{
							ResponseTokenDTO token = new ResponseTokenDTO
							{
								AccessToken = _TokenServices.generateAccessToken(u),
								RefreshToken = refreshToken
							};
							return new ResultDTO
							{
								StatusCode = 200,
								Message = "Logined Successfully",
								result = token ////
							};
						}
						else
						{
							return new ResultDTO
							{
								Message = "Something Went Wrong please try to login again",
								StatusCode = 401
							};
						}
					}
					else
						return new ResultDTO
						{
							Message = "Something Went Wrong please try to login again",
							StatusCode = 401
						};
				}
			}
			else
			{

				string refreshTokenString = await _TokenServices.generateRefreshToken(user, cancellationToken);

				//int res = await _uow.SaveChangesAsync();
				if (string.IsNullOrEmpty(refreshTokenString))
				{
					return new ResultDTO
					{
						StatusCode = 500,
						Message = "Error generating new token"
					};
				}
				else
				{
					ResponseTokenDTO token = new ResponseTokenDTO
					{
						AccessToken = _TokenServices.generateAccessToken(user),
						RefreshToken = refreshTokenString
					};
					return new ResultDTO
					{
						StatusCode = 200,
						Message = "Logined Successfully",
						result = token ////
					};
				}
			}
		}


		public async Task<ResultDTO> refreshToken(string refresh, CancellationToken cancellationToken)
		{
			return await getTokenAsync(refresh: true, refreshToken: refresh, cancellationToken: cancellationToken);
		}

		private static int GetYearsDifference(DateOnly fromDate, DateOnly toDate)
		{
			if (toDate < fromDate)
				return 0;

			int years = toDate.Year - fromDate.Year;

			// Adjust if the last year is not fully completed
			if (toDate.Month < fromDate.Month ||
			   (toDate.Month == fromDate.Month && toDate.Day < fromDate.Day))
			{
				years--;
			}

			return years;
		}
	}
}
