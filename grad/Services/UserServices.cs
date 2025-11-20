using grad.DTO;
using grad.Interfaces;
using grad.Model;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using static System.Net.WebRequestMethods;
using System.Security.Cryptography;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using grad.Repositories;

//remove comment from email sending parts after testing

namespace grad.Services
{
	public class UserServices : IUserServices
	{
		private readonly IRepository _repository;
		private readonly IRedisServices _redis;
		private readonly IEmailServices _emailServices;
		private readonly ITokenServices _TokenServices;

		public UserServices(IRepository repository, IRedisServices redis, IEmailServices emailServices,ITokenServices tokenServices)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_redis = redis ?? throw new ArgumentNullException(nameof(redis));
			_emailServices = emailServices ?? throw new ArgumentNullException(nameof(emailServices));
			_TokenServices = tokenServices as TokenServices ?? throw new ArgumentNullException(nameof(tokenServices));
		}

		public async Task<ResultDTO> register(SignupDTO ?user,Student ?student,bool Reg,CancellationToken cancellationToken)
		{
			if (Reg)
			{
				if (student == null)
				{
					return new ResultDTO
					{
						StatusCode = StatusCodes.Status400BadRequest,
						Message = "Invalid student data"
					};
				}
				else
				{
				
					object res = await _repository.CreateEntityAsync<Student>(student, cancellationToken: cancellationToken);
					if(res == null)
					{
						return new ResultDTO
						{
							StatusCode = StatusCodes.Status500InternalServerError,
							Message = "Error creating student"
						};
					}
					else
					{
						return new ResultDTO
						{
							StatusCode = StatusCodes.Status201Created,
							Message = "Student registered successfully"
						};
					}
				}
			}
			else
			{
				if (await UserExists(user.email, cancellationToken: cancellationToken))
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
					case (int)User.UserRole.Admin:
						Admin admin = new Admin
						{
							EmailorUserName = user.email,
							Password = user.password,
							Role = User.UserRole.Admin
						};
						result = await _repository.CreateEntityAsync<Admin>(admin, cancellationToken: cancellationToken);
						break;
					case (int)User.UserRole.Parent:
						Parent parent = new Parent
						{
							EmailorUserName = user.email,
							Password = user.password,
							FName = user.FName,
							LName = user.LName,
							phoneNumber = user.phoneNumber,
							Address = user.Address,
							Job = user.Job,
							Role = User.UserRole.Parent
						};
						result = await _repository.CreateEntityAsync<Parent>(parent, cancellationToken: cancellationToken);
						break;
					case (int)User.UserRole.Teacher:
						Subject subject = await _repository.GetEntityAsync<Subject>((s => s.Id.ToLower().Equals(user.SubjectID.ToLower())), cancellationToken: cancellationToken);
						if(subject == null)
						{
							return new ResultDTO
							{
								StatusCode = StatusCodes.Status400BadRequest,
								Message = "Registration Failed"
							};
						}
						Teacher teacher = new Teacher
						{
							EmailorUserName = user.email,
							Password = user.password,
							FName = user.FName,
							LName = user.LName,
							phoneNumber = user.phoneNumber,
							Address = user.Address,
							Role = User.UserRole.Teacher,
							IsActive = false,
							IsVerified = false,
						};
						result = await _repository.CreateEntityAsync<Teacher>(teacher, cancellationToken: cancellationToken);
						if(result is Teacher t)
						{
							//await _emailServices.SendTeacherRegistrationEmail(t.EmailorUserName, cancellationToken);
						}
						break;
					default:
						//throw new ArgumentOutOfRangeException("Invalid role");
						return new ResultDTO
						{
							StatusCode = StatusCodes.Status400BadRequest,
							Message = "Registration Failed"
						};
				}
				if (result == null)
					return new ResultDTO
					{
						StatusCode = StatusCodes.Status400BadRequest,
						Message = "Registration Failed"
					};
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status201Created,
					Message = "Thanks for working with us"
				};
			}
		}

		public async Task<ResultDTO> login(LoginDTO login,CancellationToken cancellationToken) //not complete yet
		{
			User user = new User
			{
				EmailorUserName = login.UsernameorEmail,
			};
			user = await _repository.GetEntityAsync<User>(u => u.EmailorUserName.ToLower().Equals(user.EmailorUserName.ToLower()),cancellationToken: cancellationToken);
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
				if (!user.IsActive)
				{
					return new ResultDTO
					{
						StatusCode = StatusCodes.Status403Forbidden,
						Message = "account is deactivated"
					};
				}
				else if (!user.IsVerified)
				{
					return new ResultDTO
					{
						StatusCode = StatusCodes.Status403Forbidden,
						Message = "account is not verified"
					};
				}
				else if (user.IsLockedOut)
				{
					return new ResultDTO
					{
						StatusCode = StatusCodes.Status403Forbidden,
						Message = "Your account is temporarily locked after several unsuccessful login attempts. You can regain access by resetting your password."
					};
				}
				else if (await getAttempts(user.EmailorUserName) >= 5) //dont forget to impletement refresh token part
				{
					user.IsLockedOut = true;
					await _repository.UpdateEntityAsync<User>(user);
					string token =  _TokenServices.generateAccessToken(user,true);
					Console.WriteLine(token);
					//await _emailServices.SendAccountLockedEmail(user.EmailorUserName,token);//TODO: add unlock link 
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
						await _repository.UpdateEntityAsync<User>(user);
						return new ResultDTO
						{
							StatusCode = StatusCodes.Status400BadRequest,
							Message = "invalid credentials"
						};
					}
					else
					{
						await storeAttempts(user.EmailorUserName, true);

						return await getTokenAsync(user,false,cancellationToken);
					}
				}
			}
		}

		

		public async Task<ResultDTO> RequestChangePass(User user, CancellationToken cancellationToken)
		{
			user = await _repository.GetEntityAsync<User>((u => u.EmailorUserName.ToLower().Equals(user.EmailorUserName.ToLower())),cancellationToken: cancellationToken);
			if (user != null)
			{
				int otpInt = RandomNumberGenerator.GetInt32(100000, 1000000);
				string otp = otpInt.ToString();
				string hashedotp = BCrypt.Net.BCrypt.HashPassword(otp);
				bool res = await storeOTP(user.EmailorUserName, hashedotp);
				if (res)
				{
					//bool sent = await _emailServices.SendOTPEmail(user.EmailorUserName, otp, cancellationToken
					//
					bool sent = true;
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
					StatusCode = 400,
					Message = "Invalid OTP data"
				};
			else
			{
				var res = await getOTP(otp.EmailorUserName);
				if (res == null)
					return new ResultDTO
					{
						StatusCode = 500,
						Message = "OTP wasnt found"
					};
				else
				{
					bool verify = BCrypt.Net.BCrypt.Verify(otp.OTP,res);
					if (verify)
					{
						var user = await _repository.GetEntityAsync<User>(u => u.EmailorUserName.ToLower().Equals(otp.EmailorUserName.ToLower()),cancellationToken:cancellationToken);
						if (user == null)
						{
							return new ResultDTO
							{
								StatusCode = StatusCodes.Status500InternalServerError,
								Message = "User not found"
							};
						}
						else if(BCrypt.Net.BCrypt.Verify(otp.NewPassword,user.Password))
						{
							return new ResultDTO
							{
								StatusCode = StatusCodes.Status400BadRequest,
								Message = "New password cannot be the same as the old password"
							};
						}
						await deleteOTP(otp.EmailorUserName);
						return await savePassword(user,otp.NewPassword,cancellationToken: cancellationToken);
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

		//public async Task<ResultDTO> ResetPassword(ResetPassDTO resetPass) //dont forget to check for password's regex
		//{
		//	throw new NotImplementedException();
		//}


		public async Task<ResultDTO> ResetPassword(ChangePasswordDTO changePassword, CancellationToken cancellationToken)
		{	
			User user = await _repository.GetEntityAsync<User>(u => u.Id.Equals(changePassword.Id), cancellationToken: cancellationToken);
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
				if(changePassword.token.IsNullOrEmpty())
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
						return await savePassword(user,changePassword.NewPassword,cancellationToken: cancellationToken);
					}
				}
                else
				{
					bool res = await _TokenServices.blacklistToken(changePassword.token);

					return await savePassword(user,changePassword.NewPassword, cancellationToken: cancellationToken);
                }
            }
		}


		public async Task<ResultDTO> ViewProfile(ProfileDTO user, CancellationToken cancellationToken)
		{
			if (user != null)
			{
				if (Enum.TryParse<User.UserRole>(user.Role, out var role))
				{
					switch (role)
					{
						case User.UserRole.Admin:
							Admin admin = await _repository.GetEntityAsync<Admin>(u => u.Id.Equals(user.Id),cancellationToken: cancellationToken);
							if(admin != null)
							{
								user.Email = admin.EmailorUserName;
								user.Name = "Admin";
							}
							break;
						case User.UserRole.Parent:
							Parent parent = await _repository.GetEntityAsync<Parent>(u => u.Id.Equals(user.Id), cancellationToken: cancellationToken);
							if (parent != null)
							{
								user.Id = parent.Id;
								user.Email = parent.EmailorUserName;
								user.Role = parent.Role.ToString();
								user.Name = $"{parent.FName} {parent.LName}";
								user.Address = parent.Address;
								user.phone = parent.phoneNumber;
							}
							break;
						case User.UserRole.Student:
							Student student = await _repository.GetEntityAsync<Student>(u => u.Id.Equals(user.Id),q=>q.Include(u=>u.parent),cancellationToken: cancellationToken);
							//Parent p = await _repository.GetEntityAsync<Parent>(u => u.Id.Equals(student.PID));
							if(student != null)
							{
								user.Id = student.Id;
								user.Email = student.EmailorUserName;
								user.Role = student.Role.ToString();
								user.Name = $"{student.FName} {student.LName}";
								user.Address = student.parent.Address;
								user.BirthDate = student.BirthDate;
								user.Disability = student.Disability.ToString();
								if (student.parent != null)
								{
									user.setParent(new Parent
									{
										EmailorUserName = student.parent.EmailorUserName,
										FName = student.parent.FName,
										phoneNumber = student.parent.phoneNumber,
										reletationShip = student.parent.reletationShip
									});
								}
							}
							break;
						default:
							return new ResultDTO
							{
								StatusCode = StatusCodes.Status403Forbidden,
								Message = "invalid action" // unknown role
							};
					}
				}
				if (user.Name.IsNullOrEmpty())
					return new ResultDTO
					{
						StatusCode = StatusCodes.Status404NotFound,
						Message = "User isn't found"
					};
				else
					return new ResultDTO
					{
						Message = "Found",
						StatusCode = StatusCodes.Status200OK,
						result = user
					};
			}
			else
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status500InternalServerError,
					Message = "Something Went Wrong"
				};
		}

		public async Task<ResultDTO> LogOut(string token,string uid ,CancellationToken cancellationToken = default) // need some improvements
		{
			throw new NotImplementedException();
			//User user = await _repository.GetEntityAsync<User>((u=>u.Id.ToLower().Equals(uid.ToLower())),q=>q.Include(u=>u.RefreshToken),cancellationToken: cancellationToken);
			//RefreshToken refreshToken = user.RefreshToken;
			//refreshToken.Revoked = true;
			//ResultDTO rDTO = await _repository.UpdateEntityAsync<RefreshToken>(refreshToken, cancellationToken);
			//if(rDTO.SuccessCode == 0)
			//{
			//	return rDTO;
			//}
			//var res = await _TokenServices.blacklistToken(token);
			//return res;
		}


		private async Task<bool> UserExists(string EmailorUserName,CancellationToken cancellationToken = default)
		{
			User user = await _repository.GetEntityAsync<User>(u => u.EmailorUserName.ToLower().Equals(EmailorUserName.ToLower()), cancellationToken: cancellationToken);
			return user != null;
		}


		private async Task<ResultDTO> savePassword(User user,string NewPassword,CancellationToken cancellationToken = default)// needs to be tested
		{
			//revoke user token
			bool res = await revokeToken(user, cancellationToken);

			if(res)
			{
				if (user.IsLockedOut)
					user.IsLockedOut = false;
				user.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword);
				bool updateRes = await _repository.UpdateEntityAsync<User>(user, cancellationToken: cancellationToken);
				if (updateRes)
				{
					//await _emailServices.SendPasswordResetSuccessEmail(user.EmailorUserName); remove comment
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
			else
			{
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status500InternalServerError,
					Message = "Failed to revoke token"
				};
			}
		}

		private async Task<bool> revokeToken(User user , CancellationToken cancellationToken)
		{
			RefreshToken token = user.RefreshToken;
			if(token == null)
			{
				return true; // if it is null so there is no session
			}
			else
			{
				token.Revoked = true;
				return await _repository.UpdateEntityAsync<RefreshToken>(token, cancellationToken);
			}
		}
		private async Task<bool> storeOTP(string emailorUserName, string hashedotp)
		{
			return await _redis.store(emailorUserName.ToLower(), hashedotp,TimeSpan.FromMinutes(10));
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


		public async Task<ResultDTO> refreshToken(RequestRefreshToken token, CancellationToken cancellationToken)
		{
			token.uid = _TokenServices.getUID(token.AccessToken);
			if(token.uid.IsNullOrEmpty())
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status400BadRequest,
					Message = "Invalid Token"
				};
			RefreshToken validateToken = await ValidateRefreshToken(token, cancellationToken);
			if (validateToken == null)
			{
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status500InternalServerError,
					Message = "Error generating new token"
				};
			}
			else
			{
				User user = validateToken.User;
				user.RefreshToken = validateToken;
				ResultDTO result = await getTokenAsync(user, true, cancellationToken);
				return result;
			}
		}

		private async Task<ResultDTO> getTokenAsync(User user, bool refresh, CancellationToken cancellationToken)
		{
			if (refresh)
			{
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status200OK,
					Message = "Token generated successfully",
					result = new ResponseTokenDTO
					{
						AccessToken = _TokenServices.generateAccessToken(user),
						RefreshToken = user.RefreshToken.Token
					}
				};
			}
			else
			{
				RefreshToken oldToken = await _repository.GetEntityAsync<RefreshToken>((t => t.CreatedById == user.Id), cancellationToken: cancellationToken);
				if (oldToken != null)
				{
					bool result = await _repository.DeleteEntityAsync<RefreshToken>(oldToken);
					if (!result)
					{
						return new ResultDTO
						{
							StatusCode = StatusCodes.Status500InternalServerError,
							Message = "Error generating new token"
						};// something wrong within delete of old token
					}
				}
				RefreshToken refreshToken = new RefreshToken
				{
					CreatedById = user.Id,
					Created = DateTime.UtcNow,
					Expires = DateTime.UtcNow.AddDays(7),
					Token = _TokenServices.generateRefreshToken(),
					Revoked = false
				};
				object res = await _repository.CreateEntityAsync<RefreshToken>(refreshToken, cancellationToken);
				if (res == null)
				{
					return new ResultDTO
					{
						StatusCode = StatusCodes.Status500InternalServerError,
						Message = "Error generating new token"
					};
				}
				else
				{
					var token = new ResponseTokenDTO
					{
						AccessToken = _TokenServices.generateAccessToken(user),
						RefreshToken = refreshToken.Token
					};
					return new ResultDTO
					{
						StatusCode = StatusCodes.Status200OK,
						Message = "Logined Successfully",
						result = token
					};
				}
			}
		}


		private async Task<RefreshToken> ValidateRefreshToken(RequestRefreshToken token, CancellationToken cancellationToken) // need some improvements
		{
			//User user = await _repository.GetEntityAsync<User>(u => u.Id.Equals(userid),"RefreshTokens", cancellationToken: cancellationToken);
			RefreshToken refreshToken = await _repository.GetEntityAsync<RefreshToken>((t => t.Token.ToLower().Equals(token.RefreshToken.ToLower()) && t.CreatedById.ToLower().Equals(token.uid.ToLower())),(q => q.Include(t => t.User)), cancellationToken);
			//bool expired = _TokenServices.(user.RefreshToken);
			if(refreshToken == null)
			{
				//await _repository.UpdateEntityAsync<User>(user, cancellationToken: cancellationToken);
				return null;
			}
			else
			{
				if (refreshToken.User == null || refreshToken.IsExpired || refreshToken.Revoked)
					return null;
				else
					return refreshToken;
			}
		}


	}
}

