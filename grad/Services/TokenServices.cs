using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using grad.DTO;
using grad.Interfaces;
using grad.Model;
using Microsoft.IdentityModel.Tokens;

namespace grad.Services
{
	public class TokenServices : ITokenServices
	{
		private readonly IRedisServices _redisServices;
		private readonly SecurityKey _secretKey;
		private readonly IConfiguration _configuration;
		public TokenServices(IRedisServices redisServices, IConfiguration configuration)
		{
			_redisServices = redisServices ?? throw new ArgumentNullException(nameof(redisServices));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_secretKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
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

		public string generateRefreshToken()
		{
			var randomBytes = new byte[64];
			using var rng = RandomNumberGenerator.Create();
			rng.GetBytes(randomBytes);
			return Convert.ToBase64String(randomBytes);
		}

		public async Task<AccessTokenDto> getTokenInfo(string token)
		{
			var validToken = await validateToken(token);
			if (validToken is AccessTokenDto tokenDto && tokenDto != null)
			{
				return tokenDto;
			}
			return null;
		}


		private async Task<object> validateToken(string token)
		{
			if (string.IsNullOrWhiteSpace(token))
				return false;

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
					return new AccessTokenDto
					{
						ID = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value,
						EmailorUserName = principal.FindFirst(ClaimTypes.Email)?.Value,
						Role = principal.FindFirst(ClaimTypes.Role)?.Value,
						expired = exp,
						remainingtime = time,
						Token = token
					};
				}
				else
					return false;
			}
			catch
			{
				return false; // invalid or tampered token
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
			var validtoken = await validateToken(token);
			if (validtoken is AccessTokenDto tokenDto && tokenDto != null)
			{
				if (!tokenDto.expired)
					return await _redisServices.store(tokenDto.Token, "blacklisted", tokenDto.remainingtime);
				else
					return true;
			}
			else
			{
				return false;
			}
		}

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
