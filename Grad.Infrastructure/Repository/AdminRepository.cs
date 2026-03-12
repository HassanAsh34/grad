using Grad.Application.AdminFeatures.Interfaces;
using Grad.Domain.Model;
using Grad.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Grad.Infrastructure.Repository
{
	public class AdminRepository : Repository,  IAdminRepository
	{
		private readonly Db_Context _context;
		public AdminRepository(Db_Context context) : base(context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}

		public async Task<User> getUserWithRefreshToken(Guid uid, CancellationToken cancellation)
		{
			return await _context.Users.Where(u => u.Id == uid).Include(u => u.RefreshToken).FirstOrDefaultAsync();
		}
	}
}
