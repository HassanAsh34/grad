using System;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading;
using Azure.Core;
using BCrypt.Net;
using grad.DTO;
using grad.Interfaces;
using grad.Model;
using grad.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using static System.Net.WebRequestMethods;

//remove comment from email sending parts after testing

namespace grad.Services
{
	public class UserServices : IUserServices
	{
		private readonly IRepository _repository;
		private readonly IRedisServices _redis;
		private readonly IEmailServices _emailServices;
		private readonly ITokenServices _TokenServices;
		private readonly IUowServices _uow;

		public UserServices(IRepository repository, IRedisServices redis, IEmailServices emailServices,ITokenServices tokenServices, IUowServices uow)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_redis = redis ?? throw new ArgumentNullException(nameof(redis));
			_emailServices = emailServices ?? throw new ArgumentNullException(nameof(emailServices));
			_TokenServices = tokenServices as TokenServices ?? throw new ArgumentNullException(nameof(tokenServices));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
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
				
					await _repository.CreateEntityAsync<Student>(student, cancellationToken: cancellationToken);
					int res = await _uow.SaveChangesAsync();
					if(res == 0)
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
						//result = await _repository.CreateEntityAsync<Admin>(admin, cancellationToken: cancellationToken);
						await _repository.CreateEntityAsync<Admin>(admin, cancellationToken: cancellationToken);
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
						//result = await _repository.CreateEntityAsync<Parent>(parent, cancellationToken: cancellationToken);
						await _repository.CreateEntityAsync<Parent>(parent, cancellationToken: cancellationToken);
						break;
					case (int)User.UserRole.Teacher:
						Subject subject = await _repository.GetEntityAsync<Subject>((s => s.Id.ToLower().Equals(user.SubjectID.ToLower())), cancellationToken: cancellationToken);
						if (subject == null)
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
						//result = await _repository.CreateEntityAsync<Teacher>(teacher, cancellationToken: cancellationToken);
						await _repository.CreateEntityAsync<Teacher>(teacher, cancellationToken: cancellationToken);
						//if (result is Teacher t)
						//{
						//	//await _emailServices.SendTeacherRegistrationEmail(t.EmailorUserName, cancellationToken);
						//}
						break;
					default:
						//throw new ArgumentOutOfRangeException("Invalid role");
						return new ResultDTO
						{
							StatusCode = StatusCodes.Status400BadRequest,
							Message = "Registration Failed"
						};
				}

				//---------------------------------------------------
				int res = await _uow.SaveChangesAsync();
				if (res == 0)
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
					_repository.UpdateEntityAsync<User>(user);
					await _uow.SaveChangesAsync();
					//string token =  _TokenServices.generateAccessToken(user,true);
					//Console.WriteLine(token);
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
					bool verify = BCrypt.Net.BCrypt.Verify(otp.OTP,res);
					if (verify)
					{
						var user = await _repository.GetEntityAsync<User>(u => u.EmailorUserName.ToLower().Equals(otp.EmailorUserName.ToLower()),cancellationToken:cancellationToken);
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

		//public async Task<ResultDTO> ResetPassword(ResetPassDTO resetPass) //dont forget to check for password's regex
		//{
		//	throw new NotImplementedException();
		//}


		public async Task<ResultDTO> ResetPassword(ChangePasswordDTO changePassword,bool resetToken, CancellationToken cancellationToken) /// we need to reconfigure the whole function?
		{
			RefreshToken RT = null;
			User user = null;
			if (resetToken)
			{
				user = await _repository.GetEntityAsync<User>(u=>u.Id == changePassword.Id,q=>q.Include(r=>r.RefreshToken),cancellationToken);
				RT = user.RefreshToken;
			}
			else
			{
				RT= await _repository.GetEntityAsync<RefreshToken>(filter: t => t.Token == changePassword.refreshToken && t.CreatedById == changePassword.Id, q => q.Include(u => u.User),
					cancellationToken: cancellationToken
				);
				user = RT.User;
			}
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
				if (!changePassword.token.IsNullOrEmpty())
				{
					await _TokenServices.blacklistToken(changePassword.token);
				}
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
				return await savePassword(user, changePassword.NewPassword, cancellationToken: cancellationToken);
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

		public async Task<ResultDTO> LogOut(string token,string refreshtoken,string uid ,CancellationToken cancellationToken = default) // need some improvements
		{
			RefreshToken t = await _repository.GetEntityAsync<RefreshToken>(filter: t => t.Token == refreshtoken && t.CreatedById == uid &&
					!t.Revoked,
				cancellationToken: cancellationToken
			);
			if (t == null)
				return new ResultDTO
				{
					StatusCode= StatusCodes.Status500InternalServerError,
					Message = "something went wrong"
				};
			else
			{
				revokeToken(t, cancellationToken);
				int res = await _uow.SaveChangesAsync();
				if (res != 0)
				{
					await _TokenServices.blacklistToken(token);
					return new ResultDTO
					{
						Message = "Logged Out",
						StatusCode = StatusCodes.Status204NoContent
					};
				}
				else
					return new ResultDTO
					{
						StatusCode = StatusCodes.Status500InternalServerError,
						Message = "Something went wrong"
					};
			}
		}


		private async Task<bool> UserExists(string EmailorUserName,CancellationToken cancellationToken = default)
		{
			User user = await _repository.GetEntityAsync<User>(u => u.EmailorUserName.ToLower().Equals(EmailorUserName.ToLower()), cancellationToken: cancellationToken);
			return user != null;
		}


		private async Task<ResultDTO> savePassword(User user,string NewPassword,CancellationToken cancellationToken = default)// we need to add the token to black list 
		{
			//revoke user token
			//bool res = await revokeToken(user.RefreshToken, cancellationToken);
			revokeToken(user.RefreshToken, cancellationToken);
			if (user.IsLockedOut)
				user.IsLockedOut = false;
			user.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword);
			_repository.UpdateEntityAsync<User>(user, cancellationToken: cancellationToken);
			int updateRes = await _uow.SaveChangesAsync();
			if (updateRes != 0)
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
			//if (res)
			//{
			//	if (user.IsLockedOut)
			//		user.IsLockedOut = false;
			//	user.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword);
			//	_repository.UpdateEntityAsync<User>(user, cancellationToken: cancellationToken);
			//	int updateRes = await _uow.SaveChangesAsync();
			//	if (updateRes != 0)
			//	{
			//		//await _emailServices.SendPasswordResetSuccessEmail(user.EmailorUserName); remove comment
			//		return new ResultDTO
			//		{
			//			StatusCode = StatusCodes.Status200OK,
			//			Message = "Password changed successfully"
			//		};
			//	}
			//	else
			//	{
			//		return new ResultDTO
			//		{
			//			StatusCode = StatusCodes.Status500InternalServerError,
			//			Message = "Failed to update password"
			//		};
			//	}
			//}
			//else
			//{
			//	return new ResultDTO
			//	{
			//		StatusCode = StatusCodes.Status500InternalServerError,
			//		Message = "Failed to revoke token"
			//	};
			//}
		//}

		private void revokeToken(RefreshToken token , CancellationToken cancellationToken)
		{
			if(token != null)
			{ 
				token.Revoked = true;
				_repository.UpdateEntityAsync(token, cancellationToken: cancellationToken);	
				//return await _repository.UpdateEntityAsync<RefreshToken>(token, cancellationToken);
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


		//public async Task<ResultDTO> refreshToken(RequestRefreshToken token, CancellationToken cancellationToken)
		//{
		//	token.uid = _TokenServices.getUID(token.AccessToken);
		//	if(token.uid.IsNullOrEmpty())
		//		return new ResultDTO
		//		{
		//			StatusCode = StatusCodes.Status400BadRequest,
		//			Message = "Invalid Token"
		//		};
		//	RefreshToken validateToken = await ValidateRefreshToken(token, cancellationToken);
		//	if (validateToken == null)
		//	{
		//		return new ResultDTO
		//		{
		//			StatusCode = StatusCodes.Status500InternalServerError,
		//			Message = "Error generating new token"
		//		};
		//	}
		//	else
		//	{
		//		User user = validateToken.User;
		//		user.RefreshToken = validateToken;
		//		ResultDTO result = await getTokenAsync(user, true, cancellationToken);
		//		return result;
		//	}
		//}

		//public async Task<ResultDTO> refreshToken(ResponseTokenDTO token, CancellationToken cancellationToken)
		//{
		//	AccessTokenDto TokenDto = await _TokenServices.getTokenInfo(token.AccessToken);
		//	string refreshToken = token.RefreshToken;
		//	object validToken = await ValidateRefreshToken(refreshToken, TokenDto.ID, cancellationToken);
		//	if (validToken is RefreshToken t && !t.CreatedById.IsNullOrEmpty())
		//	{
		//		User user = t.User;
		//		user.RefreshToken = t;
		//		ResultDTO result = await getTokenAsync(user, true, cancellationToken);
		//		return result;
		//	}
		//	else
		//		return new ResultDTO
		//		{
		//			StatusCode = StatusCodes.Status401Unauthorized,
		//			Message = "You are not authorized, please log in again."
		//		};
		//		////if (validateToken == null)
		//		////{
		//		////	return new ResultDTO
		//		////	{
		//		////		StatusCode = StatusCodes.Status500InternalServerError,
		//		////		Message = "Error generating new token"
		//		////	};
		//		////}
		//		////else
		//		////{
		//		////	User user = validateToken.User;
		//		////	user.RefreshToken = validateToken;
		//		////	ResultDTO result = await getTokenAsync(user, true, cancellationToken);
		//		////	return result;
		//		////}
		//		//return null;
		//}

		public async Task<ResultDTO> refreshToken(ResponseTokenDTO token,CancellationToken cancellationToken)
		{
			var tokenInfo = await _TokenServices.getTokenInfo(token.AccessToken);

			if (tokenInfo == null || string.IsNullOrEmpty(tokenInfo.ID))
			{
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status401Unauthorized,
					Message = "Invalid access token."
				};
			}

			var validRefreshToken = await ValidateRefreshToken(
				token.RefreshToken,
				tokenInfo.ID,
				cancellationToken
			);

			if (validRefreshToken == null)
			{
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status401Unauthorized,
					Message = "Refresh token is invalid or expired. Please log in again."
				};
			}

			var user = validRefreshToken.User;

			// optional: rotate refresh token here
			user.RefreshToken = validRefreshToken;

			return await getTokenAsync(user, true, cancellationToken);
		}


		//private async Task<object> ValidateRefreshToken(string refreshtoken,string id, CancellationToken cancellationToken) // need some improvements
		//{
		//	//User user = await _repository.GetEntityAsync<User>(u => u.Id.Equals(userid),"RefreshTokens", cancellationToken: cancellationToken);
		//	RefreshToken refreshToken = await _repository.GetEntityAsync<RefreshToken>(t => t.Token.ToLower().Equals(refreshtoken.ToLower()) && t.CreatedById.ToLower().Equals(id.ToLower()),q=>q.Include(u=>u.),cancellationToken: cancellationToken);
		//	//bool expired = _TokenServices.(user.RefreshToken);
		//	if(refreshToken == null)
		//	{
		//		//await _repository.UpdateEntityAsync<User>(user, cancellationToken: cancellationToken);
		//		return false;
		//	}
		//	else
		//	{
		//		if (refreshToken.User == null || refreshToken.IsExpired || refreshToken.Revoked)
		//			return false;
		//		else
		//			return refreshToken;
		//	}
		//}

		private async Task<RefreshToken?> ValidateRefreshToken(string refreshToken,string userId,CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(refreshToken) || string.IsNullOrWhiteSpace(userId))
				return null;

			var token = await _repository.GetEntityAsync<RefreshToken>(
				filter: t =>
					t.Token == refreshToken &&
					t.CreatedById == userId &&
					!t.Revoked,
				include: q => q.Include(t => t.User),
				cancellationToken: cancellationToken
			);

			return token;
		}
		private async Task<ResultDTO> getTokenAsync(User user, bool refresh, CancellationToken cancellationToken)
		{
			if (refresh)
			{
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status200OK,
					Message = "Token generated successfully",
					result = _TokenServices.generateAccessToken(user)
				};
			}
			else
			{
				RefreshToken refreshToken = await _repository.GetEntityAsync<RefreshToken>((t => t.CreatedById == user.Id), cancellationToken: cancellationToken);
				if (refreshToken != null)
				{
					////bool result = await _repository.DeleteEntityAsync<RefreshToken>(oldToken);
					//_repository.DeleteEntityAsync<RefreshToken>(oldToken);
					//if (!result)
					//{
					//	return new ResultDTO
					//	{
					//		StatusCode = StatusCodes.Status500InternalServerError,
					//		Message = "Error generating new token"
					//	};// something wrong within delete of old token
					//}
					refreshToken.CreatedById = user.Id;
					refreshToken.Created = DateTime.UtcNow;
					refreshToken.Expires = DateTime.UtcNow.AddDays(7);
					refreshToken.Token = _TokenServices.generateRefreshToken();
					refreshToken.Revoked = false;
					_repository.UpdateEntityAsync(refreshToken);
				}
				else
				{
					refreshToken = new RefreshToken
					{
						CreatedById = user.Id,
						Created = DateTime.UtcNow,
						Expires = DateTime.UtcNow.AddDays(7),
						Token = _TokenServices.generateRefreshToken(),
						Revoked = false
					};
					await _repository.CreateEntityAsync<RefreshToken>(refreshToken, cancellationToken);
				}
				int res = await _uow.SaveChangesAsync();
				if (res == 0)
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
						RefreshToken = refreshToken.Token
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




	}
}

