using grad.DTO;
using grad.Interfaces;
using grad.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

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
				Expires = DateTime.UtcNow.AddMinutes(15),//short living token
				SigningCredentials = creds,
			};
			var TokenHandler = new JwtSecurityTokenHandler();
			var token = TokenHandler.CreateToken(tokenDescriptor);
			return String.Concat("Bearer ", TokenHandler.WriteToken(token));
		}

		public string generateRefreshToken()
		{
			var randomBytes = new byte[64];
			using var rng = RandomNumberGenerator.Create();
			rng.GetBytes(randomBytes);
			return Convert.ToBase64String(randomBytes);
		}

		public string getUID(string token)
		{
			if(token == null)
				return null;
			if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
				token = token.Substring("Bearer ".Length).Trim();
			var handler = new JwtSecurityTokenHandler();
			var jwt = handler.ReadJwtToken(token);
			var idClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.NameId);
			return idClaim?.Value;
		}

		public async Task<bool> IsTokenBlacklisted(string token)
		{
			if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
				token = token.Substring("Bearer ".Length).Trim();
			var res = await _redisServices.get(token);
			return res != null;
		}

		public async Task<bool> blacklistToken(string token)
		{
			if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
				token = token.Substring("Bearer ".Length).Trim();
			TimeSpan expire = GetTokenExpiry(token);
			Console.WriteLine(expire);
			return await _redisServices.store(token, "blacklisted", expire);
		}

		private TimeSpan GetTokenExpiry(string token)
		{
			if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
				token = token.Substring("Bearer ".Length).Trim();
			var handler = new JwtSecurityTokenHandler();
			var jwt = handler.ReadJwtToken(token);
			var expClaim = jwt.Claims.FirstOrDefault(c => c.Type == "exp");
			if (expClaim == null)
				return TimeSpan.Zero;

			var expUnix = long.Parse(expClaim.Value);
			var expDate = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
			var remaining = expDate - DateTime.UtcNow;

			return remaining;
		}

		


	}
}
