using Grad.Application.Auth.Interfaces;
using Grad.Domain.Model;
using Grad.Domain.Enums;
using Grad.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Grad.Infrastructure.Repository
{
	public class AuthRepository : Repository, IAuthRepository
	{
		private readonly Db_Context _context;
		public AuthRepository(Db_Context context) : base(context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}

		public async Task<User> getUserWithRefreshToken(Guid ?uid, CancellationToken CT)
		{
			return uid != null ? await _context.Users.Where(u => u.Id == uid).Include(u => u.RefreshToken).FirstOrDefaultAsync(CT) : null;
		}

		public async Task<bool> IsUserExist(string? email = "", Guid? uid = null, CancellationToken CT = default)
		{
			return await base.GetEntityAsync<User>(u => u.EmailorUserName.ToLower().Equals(email.ToLower().ToLower()) || u.Id == uid,CT) != null;
		}

		public async Task<string> getEmail(string email,CancellationToken CT = default)
		{
			User user = await base.GetEntityAsync<User>(u => u.EmailorUserName.ToLower().Equals(email.ToLower().ToLower()),CT);
			if (user != null)
				switch (user.Role)
				{
					case UserRole.Student:
						Student s = await _context.Students.Where(s => s.Id == user.Id).Include(s => s.parent).FirstOrDefaultAsync(CT);
						if (s.parent != null)
							return s.parent.EmailorUserName;
						else
							return s.EmailorUserName;
					default:
						return user.EmailorUserName;
				}
			else
				return string.Empty;
		}
	}
}
