using Grad.Application.Common.Interfaces;
using  Grad.Infrastructure.Persistence;

namespace  Grad.Infrastructure.UnitOfWork
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
