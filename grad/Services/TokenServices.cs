using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using grad.DTO;
using grad.Interfaces;
using grad.Model;
using grad.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


namespace grad.Services
{
	public class TokenServices : ITokenServices
	{
		private readonly IRedisServices _redisServices;
		private readonly SecurityKey _secretKey;
		private readonly IConfiguration _configuration;
		private readonly IRepository _repository;
		private readonly IUowServices _uow;
		public TokenServices(IRedisServices redisServices, IConfiguration configuration,IRepository repository,IUowServices uow)
		{
			_redisServices = redisServices ?? throw new ArgumentNullException(nameof(redisServices));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_secretKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));	
		}
		public string generateAccessToken(User user, bool reset = false)
		{
			List<Claim> Claims = null;
			if (reset)
				Claims = new List<Claim>
						{
							new Claim(JwtRegisteredClaimNames.Email, user.EmailorUserName),
							new Claim(JwtRegisteredClaimNames.NameId, user.Id),
							new Claim(ClaimTypes.Role,"ResetPassword"),
						};
			else
			{
				Claims = new List<Claim>
						{
							new Claim(JwtRegisteredClaimNames.Email, user.EmailorUserName),
							new Claim(JwtRegisteredClaimNames.NameId, user.Id)
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
						break;
					case Teacher t:
						Claims.AddRange(new List<Claim>{
							new Claim(JwtRegisteredClaimNames.GivenName, $"{t.FName} {t.LName}"),
							new Claim("SubjectID", t.SubjectFK)
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
				Expires = System.DateTime.UtcNow.AddMinutes(15),//short living token
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
					new Claim(JwtRegisteredClaimNames.NameId, refreshToken.Id),
					new Claim(JwtRegisteredClaimNames.Sub,user.Id),
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

		public async Task<RefreshTokenDTO> getTokenInfo(string token, CancellationToken cancellationToken = default)
		{
			var validToken = await validateRefreshToken(token,cancellationToken: cancellationToken);
			if (validToken is RefreshTokenDTO refreshDto && refreshDto != null)
			{
				return refreshDto;
			}
			else
			{
				return null;
			}
		}

		public async Task<bool> IsTokenBlacklisted(string token)
		{
			if (!token.IsNullOrEmpty())
			{
				var res = await _redisServices.get(token);
				return res != null;
			}
			else
			{ return false; }
		}

		public async Task<bool> blacklistToken(string token)
		{
			//var validtoken = await validateRefreshToken(token);
			//if (validtoken is AccessTokenDto tokenDto && tokenDto != null)
			//{
			//	if (!tokenDto.expired)
			//		return await _redisServices.store(tokenDto.Token, "blacklisted", tokenDto.remainingtime);
			//	else
			//		return true;
			//}
			//else
			//{
			//	return false;
			//}
			throw new NotImplementedException();
		}

		private string generateRefreshTokenKey()
		{
			var randomBytes = new byte[64];
			using var rng = RandomNumberGenerator.Create();
			rng.GetBytes(randomBytes);
			return Convert.ToBase64String(randomBytes);
		}


		private async Task<object> validateRefreshToken(string token, bool revoke = false, CancellationToken cancellationToken = default)
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
				ValidateLifetime = false, // 🔑 allow expired tokens
										  //ValidIssuer = _config["Jwt:Issuer"],
										  //ValidAudience = _config["Jwt:Audience"],
										  //IssuerSigningKey = new SymmetricSecurityKey(
										  //Encoding.UTF8.GetBytes(_config["Jwt:Key"]))
			};

			try
			{
				var principal = handler.ValidateToken(
					token,
					validationParameters,
					out _
				);

				bool exp = false;
				TimeSpan time = TimeSpan.Zero;
				var jwt = handler.ReadJwtToken(token);
				if (jwt.ValidTo < System.DateTime.UtcNow)
					exp = true;
				else
					time = jwt.ValidTo - System.DateTime.UtcNow;

				//List<Claim> claims = jwt.Claims.ToList();

				//claims.ForEach(c =>
				//{
				//	Console.WriteLine($"{c.Type}||{c.Value.ToString()}");
				//});

				if (!await IsTokenBlacklisted(token))
				{
					RefreshTokenDTO refreshTokenDTO = new RefreshTokenDTO
					{
						Id = jwt.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value,
						Uid = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value,
						TokenKey = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value,
					};
					User user = await _repository.GetEntityAsync<User>(u => u.Id.ToLower().Equals(refreshTokenDTO.Uid), include: q => q.Include(u => u.RefreshToken), cancellationToken: cancellationToken);
					if (user != null)
					{
						refreshTokenDTO.role = (int)user.Role;
						RefreshToken refreshToken = user?.RefreshToken;
						bool validRefresh = false;
						if (refreshToken != null)
						{
							validRefresh = BCrypt.Net.BCrypt.Verify(refreshTokenDTO.TokenKey, refreshToken.TokenKey);
						}
						if (!validRefresh)
							return null;
						else
						{
							if (revoke)
								return refreshToken;
							else
								return refreshTokenDTO;
						}
					}
					else
						return null;
				}
				else
					return null;
			}
			catch
			{
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
