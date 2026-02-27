using grad.Application.Common.Interfaces;
using grad.Infrastructure.Persistence;

namespace grad.Infrastructure.UnitOfWork
{
	public class UowServices : IUowServices
	{
		private readonly Db_Context _context;

		public UowServices(Db_Context context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}

		public async Task<int> SaveChangesAsync()
		{
			return await _context.SaveChangesAsync();
		}
	}
}
