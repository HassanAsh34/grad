using System.Security.Cryptography;
using grad.Application.Auth.DTOs;
using grad.Application.Auth.Interfaces;
using grad.Application.Common.DTOs;
using grad.Application.Common.Interfaces;
using grad.Application.parent.DTOs;
using grad.Domain.Enums;
using grad.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace grad.Application.Auth.Services
{
	public class AuthServices : IAuthServices
	{
		private readonly IRepository _repository;
		private readonly IRedisServices _redis;
		private readonly IEmailServices _emailServices;
		private readonly ITokenServices _TokenServices;
		private readonly IUowServices _uow;
		private readonly ICloudinaryServices _cloudinaryServices;

		public AuthServices(IRepository repository, IRedisServices redis, IEmailServices emailServices, ITokenServices tokenServices, IUowServices uow, ICloudinaryServices cloudinaryServices)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_redis = redis ?? throw new ArgumentNullException(nameof(redis));
			_emailServices = emailServices ?? throw new ArgumentNullException(nameof(emailServices));
			_TokenServices = tokenServices as TokenServices ?? throw new ArgumentNullException(nameof(tokenServices));
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
						StatusCode = StatusCodes.Status400BadRequest,
						Message = "Invalid student data"
					};
				}
				else
				{
					Parent parent = await _repository.GetEntityAsync<Parent>(p => p.Id == studentDTO.p_Id, cancellationToken: cancellationToken);
					if (parent == null)
						return new ResultDTO
						{
							StatusCode = StatusCodes.Status401Unauthorized,
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
						StatusCode = StatusCodes.Status409Conflict,
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
						if (parent.phoneNumber.IsNullOrEmpty())
							return new ResultDTO
							{
								StatusCode = StatusCodes.Status400BadRequest,
								Message = "Phone number cant be empty"
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
						if (teacher.phoneNumber.IsNullOrEmpty())
							return new ResultDTO
							{
								StatusCode = StatusCodes.Status400BadRequest,
								Message = "Phone number cant be empty"
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
								StatusCode = StatusCodes.Status400BadRequest,
								Message = "Invalid birth date"
							};
						}
						_repository.CreateEntityAsync<Student>(student, cancellationToken: cancellationToken);
						u = student;
						break;
					default:
						return new ResultDTO
						{
							StatusCode = StatusCodes.Status400BadRequest,
							Message = "Registration Failed"
						};
				}
				file = user.file;
			}
			int res = await _uow.SaveChangesAsync();
			if (res == 0)
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status400BadRequest,
					Message = "Registration Failed"
				};
			else
			{
				if (file != null)
					await storeImage(u, file, cancellationToken);
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status201Created,
					Message = "Thanks for working with us"
				};
			}
		}

		private async Task storeImage(User user, IFormFile file, CancellationToken cancellationToken = default) // implement cloudinary
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
				user.ProfilePicture = await _cloudinaryServices.UploadImageAsync(file, "user", publicId, cancellationToken);
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
					StatusCode = StatusCodes.Status400BadRequest,
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
							StatusCode = StatusCodes.Status403Forbidden,
							Message = "account is banned"
						};
					case Status.Pending:
						return new ResultDTO
						{
							StatusCode = StatusCodes.Status403Forbidden,
							Message = "account is pending approval"
						};
					case Status.Inactive:
						return new ResultDTO
						{
							StatusCode = StatusCodes.Status403Forbidden,
							Message = "account is deactivated"
						};
					case Status.locked:
						return new ResultDTO
						{
							StatusCode = StatusCodes.Status403Forbidden,
							Message = "Your account is temporarily locked after several unsuccessful login attempts. You can regain access by resetting your password."
						};
					case Status.Active:
						break;
				}
				//if (!user.IsActive)
				//{
				//	return new ResultDTO
				//	{
				//		StatusCode = StatusCodes.Status403Forbidden,
				//		Message = "account is deactivated"
				//	};
				//}
				//else if (!user.IsVerified)
				//{
				//	return new ResultDTO
				//	{
				//		StatusCode = StatusCodes.Status403Forbidden,
				//		Message = "account is not verified"
				//	};
				//}
				//else if (user.IsLockedOut)
				//{
				//	return new ResultDTO
				//	{
				//		StatusCode = StatusCodes.Status403Forbidden,
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
						StatusCode = StatusCodes.Status400BadRequest,
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
							StatusCode = StatusCodes.Status400BadRequest,
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



		public async Task<ResultDTO> RequestChangePass(User user, CancellationToken cancellationToken)
		{
			user.EmailorUserName = user.EmailorUserName.ToLower().Trim();
			user = await _repository.GetEntityAsync<User>((u => u.EmailorUserName.ToLower().Equals(user.EmailorUserName)), cancellationToken: cancellationToken);
			if (user != null)
			{
				int otpInt = RandomNumberGenerator.GetInt32(100000, 1000000);
				string otp = otpInt.ToString();
				string hashedotp = BCrypt.Net.BCrypt.HashPassword(otp);
				bool res = await storeOTP(user.EmailorUserName, hashedotp);
				if (res)
				{
					string email = string.Empty;
					bool sent = false;
					if (user.Role == UserRole.Student)
					{
						Student student = await _repository.GetEntityAsync<Student>(s => s.Id == user.Id, include: q => q.Include(s => s.parent), cancellationToken: cancellationToken);
						email = student.parent != null ? student.parent.EmailorUserName : student.EmailorUserName;
					}
					else
						email = user.EmailorUserName;
					sent = await _emailServices.SendOTPEmail(email, otp, cancellationToken); //keep it for testing
																							 //sent = true;
					Console.WriteLine($"otp {otp}");
					//
					if (sent)
						return new ResultDTO
						{
							StatusCode = StatusCodes.Status200OK,
							Message = "If the email exists, an OTP has been sent."
						};
					else
					{
						return new ResultDTO
						{
							StatusCode = StatusCodes.Status500InternalServerError,
							Message = "Error sending OTP"
						};
					}
				}
				else
				{
					return new ResultDTO
					{
						StatusCode = StatusCodes.Status500InternalServerError,
						Message = "Error sending OTP"
					};
				}
			}
			else
			{
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status200OK,
					Message = "If the email exists, an OTP has been sent."
				};
			}
		}

		public async Task<ResultDTO> ResetPasswordOTP(OTP_DTO otp, CancellationToken cancellationToken)
		{
			if (otp == null)
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status400BadRequest,
					Message = "Invalid OTP data"
				};
			else
			{
				var res = await getOTP(otp.EmailorUserName);
				if (res == null)
					return new ResultDTO
					{
						StatusCode = StatusCodes.Status400BadRequest,
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
								StatusCode = StatusCodes.Status500InternalServerError,
								Message = "Something went wrong"
							};
						}
						//else if(BCrypt.Net.BCrypt.Verify(otp.NewPassword,user.Password))
						//{
						//	return new ResultDTO
						//	{
						//		StatusCode = StatusCodes.Status400BadRequest,
						//		Message = "New password cannot be the same as the old password"
						//	};
						//}
						await deleteOTP(otp.EmailorUserName);
						//return await savePassword(user,otp.,cancellationToken: cancellationToken);
						return new ResultDTO
						{
							Message = "Verified",
							StatusCode = StatusCodes.Status200OK,
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
							StatusCode = StatusCodes.Status400BadRequest,
							Message = "Invalid OTP"
						};
					}
				}
			}
		}



		public async Task<ResultDTO> ResetPassword(ChangePasswordDTO changePassword, bool resetToken, CancellationToken cancellationToken) /// we need to reconfigure the whole function?
		{
			RefreshToken RT = null;
			//User user = null;
			User user = await _repository.GetEntityAsync<User>(u => u.Id == changePassword.Id, q => q.Include(r => r.RefreshToken), cancellationToken);
			RT = user.RefreshToken;

			//if (resetToken) {




			//if (resetToken)
			//{
			//}
			//else
			//{
			//	RefreshTokenDTO tokenDTO = await _TokenServices.getTokenInfo(changePassword.refreshToken, true, cancellationToken);
			//	RT= await _repository.GetEntityAsync<RefreshToken>(filter:t=>t.CreatedById == changePassword.Id, q => q.Include(u => u.User),
			//		cancellationToken: cancellationToken
			//	);
			//	if(BCrypt.Net.BCrypt.Verify(tokenDTO.TokenKey, RT.TokenKey))
			//		user = RT.User;
			//	else
			//	{
			//		return new ResultDTO
			//		{
			//			StatusCode = StatusCodes.Status400BadRequest,
			//			Message = ""
			//		};
			//	}
			//}
			if (user == null)
			{
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status500InternalServerError,
					Message = "User not found"
				};
			}
			else
			{
				if (RT != null)
				{
					RT.Revoked = true;
				}
				//if (!changePassword.token.IsNullOrEmpty())
				//{
				//	await _TokenServices.blacklistToken(changePassword.token);
				//}
				if (!resetToken)
				{
					if (!changePassword.OldPassword.IsNullOrEmpty())
					{
						bool verified = BCrypt.Net.BCrypt.Verify(changePassword.OldPassword, user.Password);
						if (!verified)
						{
							return new ResultDTO
							{
								StatusCode = StatusCodes.Status400BadRequest,
								Message = "Wrong password"
							};
						}
						else
						{
							if (changePassword.OldPassword.ToLower().Equals(changePassword.NewPassword.ToLower()))
								return new ResultDTO
								{
									StatusCode = StatusCodes.Status400BadRequest,
									Message = "New password should not match the old password"
								};
						}
					}
					else
						return new ResultDTO
						{
							StatusCode = StatusCodes.Status400BadRequest,
							Message = "Old password cant be empty"
						};
				}
				return await savePassword(user, changePassword.NewPassword, changePassword.token, cancellationToken: cancellationToken);
			}
		}

		public async Task<ResultDTO> LogOut(LogoutDTO logout, CancellationToken cancellationToken = default) // need some improvements
		{
			//throw new NotImplementedException();
			//_TokenServices.blacklistToken(token);
			bool res = await _TokenServices.RevokeRefreshToken(logout.RefreshToken, cancellationToken);
			if (res)
			{
				bool blackListed = await _TokenServices.blacklistToken(logout.accessToken);
				if (blackListed)
				{
					return new ResultDTO
					{
						Message = "Logged Out",
						StatusCode = StatusCodes.Status204NoContent
					};
				}
				else
				{
					return new ResultDTO
					{
						StatusCode = StatusCodes.Status500InternalServerError,
						Message = "Something went wrong"
					};
				}
			}
			else
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status500InternalServerError,
					Message = "Something went wrong"
				};
		}

		public async Task<bool> UserExists(string? EmailorUserName = "", Guid? uid = null, CancellationToken cancellationToken = default)
		{
			User user;
			EmailorUserName = EmailorUserName.ToLower().Trim();
			if (uid != null)
			{
				user = await _repository.GetEntityAsync<User>(
					u => u.Id == uid || u.EmailorUserName.ToLower().Equals(EmailorUserName),
					cancellationToken: cancellationToken);
			}
			else
			{
				user = await _repository.GetEntityAsync<User>(
					u => u.EmailorUserName.ToLower().Equals(EmailorUserName),
					cancellationToken: cancellationToken);
			}

			return user != null;
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
					StatusCode = StatusCodes.Status200OK,
					Message = "Password changed successfully"
				};
			}
			else
			{
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status500InternalServerError,
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
				if (refreshToken.IsNullOrEmpty())
				{
					return new ResultDTO
					{
						Message = "Something Went Wrong please try to login again",
						StatusCode = StatusCodes.Status401Unauthorized
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
								StatusCode = StatusCodes.Status200OK,
								Message = "Logined Successfully",
								result = token ////
							};
						}
						else
						{
							return new ResultDTO
							{
								Message = "Something Went Wrong please try to login again",
								StatusCode = StatusCodes.Status401Unauthorized
							};
						}
					}
					else
						return new ResultDTO
						{
							Message = "Something Went Wrong please try to login again",
							StatusCode = StatusCodes.Status401Unauthorized
						};
				}
			}
			else
			{

				string refreshTokenString = await _TokenServices.generateRefreshToken(user, cancellationToken);

				//int res = await _uow.SaveChangesAsync();
				if (refreshTokenString.IsNullOrEmpty())
				{
					return new ResultDTO
					{
						StatusCode = StatusCodes.Status500InternalServerError,
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
						StatusCode = StatusCodes.Status200OK,
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
