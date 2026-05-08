using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Grad.Application.Auth.Interfaces;
using Grad.Domain.Model;
using Grad.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Grad.Application.Auth.DTOs;
using Microsoft.Extensions.Logging;


namespace grad.Application.Auth.Services
{
	public class TokenServices : ITokenServices
	{
		private readonly IRedisServices _redisServices;
		private readonly SecurityKey _secretKey;
		private readonly IConfiguration _configuration;
		private readonly IAuthRepository _repository;
		private readonly IUowServices _uow;
		private readonly ILogger<TokenServices> _logger;
		public TokenServices(IRedisServices redisServices, IConfiguration configuration,IAuthRepository authRepository,IUowServices uow, ILogger<TokenServices> logger)
		{
			_redisServices = redisServices ?? throw new ArgumentNullException(nameof(redisServices));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_secretKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
			_repository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));	
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}
		public string generateAccessToken(User user, bool reset = false)
		{
			List<Claim> Claims = null;
			if (reset)
				Claims = new List<Claim>
						{
							new Claim(JwtRegisteredClaimNames.Email, user.EmailorUserName),
							new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString().ToString()),
							new Claim(ClaimTypes.Role,"ResetPassword"),
						};
			else
			{
				Claims = new List<Claim>
						{
							new Claim(JwtRegisteredClaimNames.Email, user.EmailorUserName),
							new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString())
						};
				switch (user)
				{
					case Parent p:
						Claims.AddRange(new List<Claim>
						{
							new Claim(JwtRegisteredClaimNames.GivenName, $"{p.FName} {p.LName}"),
							new Claim("Phone", p.phoneNumber)
						});
						break;
					case Admin a:
						Claims.Add(new Claim(JwtRegisteredClaimNames.GivenName, "Admin"));
						break;
					case Student s:
						Claims.Add(new Claim(JwtRegisteredClaimNames.GivenName, $"{s.FName} {s.LName}"));
						Claims.Add(new Claim("Disability",s.Disability.ToString()));
						break;
					case Teacher t:
						Claims.AddRange(new List<Claim>{
							new Claim(JwtRegisteredClaimNames.GivenName, $"{t.FName} {t.LName}"),
							//new Claim("SubjectID", t.SubjectFK.ToString())
						});
					deafult:
						Claims.Add(new Claim(JwtRegisteredClaimNames.GivenName, "User"));
						break;
				}
				Claims.Add(new Claim(ClaimTypes.Role, user.Role.ToString()));
			}
			var creds = new SigningCredentials(_secretKey, SecurityAlgorithms.HmacSha256Signature);
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(Claims),
				Expires = reset ? System.DateTime.UtcNow.AddMinutes(30) : System.DateTime.UtcNow.AddMinutes(15),//short living token
				SigningCredentials = creds,
			};
			var TokenHandler = new JwtSecurityTokenHandler();
			return TokenHandler.WriteToken(TokenHandler.CreateToken(tokenDescriptor));
			//var token = TokenHandler.CreateToken(tokenDescriptor);
			//return String.Concat("Bearer ", TokenHandler.WriteToken(token));
		}

		
		public async Task<string> generateRefreshToken(User user,CancellationToken cancellationToken)
		{
			string TokenKey = generateRefreshTokenKey();
			RefreshToken refreshToken = await _repository.GetEntityAsync<RefreshToken>((t => t.CreatedById == user.Id), cancellationToken: cancellationToken);
			if (refreshToken != null)
			{
				refreshToken.TokenKey = BCrypt.Net.BCrypt.HashPassword(TokenKey);
				refreshToken.Revoked = false;
				refreshToken.CreatedAt = DateTime.UtcNow;
				_repository.UpdateEntityAsync(refreshToken);
			}
			else
			{
				refreshToken = new RefreshToken
				{
					CreatedById = user.Id,
					//Created = DateTime.UtcNow,
					//Expires = DateTime.UtcNow.AddDays(7),
					TokenKey = BCrypt.Net.BCrypt.HashPassword(TokenKey),
					Revoked = false
				};
				_repository.CreateEntityAsync<RefreshToken>(refreshToken, cancellationToken);
			}
			int res = await _uow.SaveChangesAsync();
			if (res <= 0)
				return String.Empty;
			else
			{
				List<Claim> Claims = new List<Claim>
				{
					new Claim(JwtRegisteredClaimNames.Email, user.EmailorUserName),
					new Claim(JwtRegisteredClaimNames.NameId, refreshToken.Id.ToString()),
					new Claim(JwtRegisteredClaimNames.Sub,user.Id.ToString()),
					new Claim(JwtRegisteredClaimNames.Jti,TokenKey)
				};
				var creds = new SigningCredentials(_secretKey, SecurityAlgorithms.HmacSha256Signature);
				var tokenDescriptor = new SecurityTokenDescriptor
				{
					Subject = new ClaimsIdentity(Claims),
					Expires = System.DateTime.UtcNow.AddDays(7),//long living token
					SigningCredentials = creds,
				};
				var TokenHandler = new JwtSecurityTokenHandler();
				return TokenHandler.WriteToken(TokenHandler.CreateToken(tokenDescriptor));
			}

		}


		public async Task<bool> RevokeRefreshToken(string RefreshToken,CancellationToken cancellationToken)
		{
			var Token = await validateRefreshToken(RefreshToken,revoke:true,cancellationToken:cancellationToken);
			if(Token is RefreshToken refreshToken && refreshToken != null)
			{
				refreshToken.Revoked = true;
				_repository.UpdateEntityAsync(refreshToken);
				int res = await _uow.SaveChangesAsync();
				return res > 0;
			}
			else
			{
				return false;
			}
		}



		public async Task<RefreshTokenDTO> getTokenInfo(string token,bool refresh, CancellationToken cancellationToken = default)
		{
			RefreshTokenDTO validToken = await validateRefreshToken(token,refresh: refresh,cancellationToken: cancellationToken) as RefreshTokenDTO;
			if (validToken != null)
			{
				return validToken;
			}
			else
			{
				return null;
			}
		}

		public async Task<bool> IsTokenBlacklisted(string token)
		{
			if (!string.IsNullOrEmpty(token))
			{
				string res = await _redisServices.get(token);
				return string.IsNullOrEmpty(res) ? false : true;
			}
			else
			{ 
				return true; 
			}
		}

		public async Task<bool> blacklistToken(string token,int remainingTime = 30)
		{
			if (!string.IsNullOrEmpty(token))
			{
				bool res = await _redisServices.store(token, "blacklisted", TimeSpan.FromMinutes(remainingTime));
				return res;
			}
			else
			{ return false; }
		}

		private string generateRefreshTokenKey()
		{
			var randomBytes = new byte[32];
			using var rng = RandomNumberGenerator.Create();
			rng.GetBytes(randomBytes);
			return Convert.ToBase64String(randomBytes);
		}


		private async Task<object> validateRefreshToken(string token,bool refresh = false,bool revoke = false, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(token))
				return null;

			if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
				token = token.Substring("Bearer ".Length).Trim();

			var handler = new JwtSecurityTokenHandler();
			var validationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = _secretKey,
				ValidateIssuer = false,
				ValidateAudience = false,
				ValidateLifetime = false, // allow expired tokens
			};

			try
			{
				// Validate signature/structure (this is where exceptions are thrown when token is invalid)
				var principal = handler.ValidateToken(token, validationParameters, out _);

				var jwt = handler.ReadJwtToken(token);

				// Debug: show claims
				var nameIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.NameId)?.Value.Trim();
				var subClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value.Trim();
				var jtiClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value.Trim();

				_logger.LogDebug("ValidateRefreshToken claims => nameid: {NameId}, sub: {Sub}, jti present: {JtiPresent}", nameIdClaim, subClaim, jtiClaim != null);

				if (subClaim == null || jtiClaim == null)
				{
					_logger.LogWarning("ValidateRefreshToken: missing required claims (sub or jti)");
					return null;
				}

				RefreshTokenDTO refreshTokenDTO = null;
				if (Guid.TryParse(nameIdClaim, out Guid Tid) && Guid.TryParse(subClaim, out Guid Uid))
				{
					refreshTokenDTO = new RefreshTokenDTO
					{
						Id = Tid,
						Uid = Uid,
						TokenKey = jtiClaim,
					};
				}
				else
				{
					return null;
				}


				// Ensure we retrieve the user with included RefreshToken

				User user = await _repository.getUserWithRefreshToken(refreshTokenDTO.Uid,cancellationToken);

				if (user == null)
				{
					_logger.LogWarning("ValidateRefreshToken: user not found for uid {UserId}", refreshTokenDTO.Uid);
					return null;
				}

				var refreshToken = user.RefreshToken;
				if (refreshToken == null)
				{
					_logger.LogWarning("ValidateRefreshToken: user {UserId} has no RefreshToken record", refreshTokenDTO.Uid);
					return null;
				}

				_logger.LogDebug("ValidateRefreshToken: stored hashed TokenKey length: {TokenKeyLength}", refreshToken.TokenKey?.Length ?? 0);

				bool validRefresh = false;
				try
				{
					// Debug: explicitly log the raw jti value length and a safe sample (do not log full sensitive tokens in production)
					_logger.LogDebug("ValidateRefreshToken: comparing raw jti length={JtiLength}", refreshTokenDTO.TokenKey?.Length);

					if (!string.IsNullOrEmpty(refreshToken.TokenKey))
						validRefresh = BCrypt.Net.BCrypt.Verify(refreshTokenDTO.TokenKey, refreshToken.TokenKey);
				}
				catch (Exception exVerify)
				{
					_logger.LogError(exVerify, "ValidateRefreshToken: BCrypt.Verify threw an exception");
					validRefresh = false;
				}

				_logger.LogDebug("ValidateRefreshToken: BCrypt.Verify result: {IsValid}", validRefresh);

				if (!validRefresh)
					return null;

				refreshTokenDTO.role = (int)user.Role;
				if(refresh && refreshToken.Revoked)
				{
					_logger.LogWarning("ValidateRefreshToken: refresh token has been revoked for user {UserId}", refreshTokenDTO.Uid);
					return null;
				}
				else if (revoke)
					return refreshToken;
				else
					return refreshTokenDTO;
			}
			catch (Exception ex)
			{
				// Log the real exception so you can see why ValidateToken failed
				_logger.LogError(ex, "ValidateRefreshToken: exception validating token");
				return null; // invalid or tampered token
			}
		}

		//private async Task<object> validateToken(string token,bool refresh = false,bool revoke = false,CancellationToken cancellationToken = default)
		//{
		//	if (string.IsNullOrWhiteSpace(token))
		//		return false;

		//	if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
		//		token = token.Substring("Bearer ".Length).Trim();

		//	var handler = new JwtSecurityTokenHandler();
		//	var validationParameters = new TokenValidationParameters
		//	{
		//		ValidateIssuerSigningKey = true,
		//		IssuerSigningKey = _secretKey,
		//		ValidateIssuer = false,
		//		ValidateAudience = false,
		//		ValidateLifetime = false, // 🔑 allow expired tokens
		//								  //ValidIssuer = _config["Jwt:Issuer"],
		//								  //ValidAudience = _config["Jwt:Audience"],
		//								  //IssuerSigningKey = new SymmetricSecurityKey(
		//								  //Encoding.UTF8.GetBytes(_config["Jwt:Key"]))
		//	};

		//	try
		//	{
		//		var principal = handler.ValidateToken(
		//			token,
		//			validationParameters,
		//			out _
		//		);

		//		bool exp = false;
		//		TimeSpan time = TimeSpan.Zero;
		//		var jwt = handler.ReadJwtToken(token);
		//		if (jwt.ValidTo < System.DateTime.UtcNow)
		//			exp = true;
		//		else
		//			time = jwt.ValidTo - System.DateTime.UtcNow;

		//		//List<Claim> claims = jwt.Claims.ToList();

		//		//claims.ForEach(c =>
		//		//{
		//		//	Console.WriteLine($"{c.Type}||{c.Value.ToString()}");
		//		//});

		//		if (!await IsTokenBlacklisted(token))
		//		{
		//			if (refresh)
		//			{
		//				RefreshTokenDTO refreshTokenDTO = new RefreshTokenDTO
		//				{
		//					Id = principal.FindFirst(JwtRegisteredClaimNames.NameId)?.Value,
		//					TokenKey = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value,
		//					Uid = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
		//				};
		//				User user = await _repository.GetEntityAsync<User>(u => u.Id.ToLower().Equals(refreshTokenDTO.Uid),include:q=>q.Include(u=>u.RefreshToken),cancellationToken: cancellationToken);
		//				if (user != null)
		//				{
		//					refreshTokenDTO.role = (int)user.Role;
		//					RefreshToken refreshToken = user?.RefreshToken;
		//					bool validRefresh = false;
		//					if (refreshToken != null) 
		//					{
		//						validRefresh = BCrypt.Net.BCrypt.Verify(refreshTokenDTO.TokenKey, refreshToken.TokenKey) && !refreshToken.Revoked;
		//					}
		//					if (!validRefresh)
		//						return false;
		//					else
		//					{
		//						if (revoke)
		//							return refreshToken;
		//						else
		//							return refreshTokenDTO;
		//					}
		//				}
		//				else
		//					return false;
		//			}
		//			else
		//				return new AccessTokenDto
		//				{
		//					ID = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value,
		//					EmailorUserName = principal.FindFirst(ClaimTypes.Email)?.Value,
		//					Role = principal.FindFirst(ClaimTypes.Role)?.Value,
		//					expired = exp,
		//					remainingtime = time,
		//					Token = token
		//				};
		//		}
		//		else
		//			return false;
		//	}
		//	catch
		//	{
		//		return false; // invalid or tampered token
		//	}
		//}





		//private TimeSpan GetTokenExpiry(string token)
		//{
		//	if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
		//		token = token.Substring("Bearer ".Length).Trim();
		//	var handler = new JwtSecurityTokenHandler();
		//	var jwt = handler.ReadJwtToken(token);
		//	var expClaim = jwt.Claims.FirstOrDefault(c => c.Type == "exp");
		//	if (expClaim == null)
		//		return TimeSpan.Zero;

		//	var expUnix = long.Parse(expClaim.Value);
		//	var expDate = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
		//	var remaining = expDate - DateTime.UtcNow;

		//	return remaining;
		//}




	}
}
