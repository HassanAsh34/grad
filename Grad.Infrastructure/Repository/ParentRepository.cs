using Grad.Application.ParentFeatures.Interfaces;
using Grad.Domain.Model;
using Grad.Infrastructure.Persistence;

namespace Grad.Infrastructure.Repository
{
	public class ParentRepository : Repository, IParentRepository
	{
		public ParentRepository(Db_Context context) : base(context)
		{
		}

		public async Task<IEnumerable<Student>> showChildren(Guid id, CancellationToken cancellationToken)
		{
			return await base.GetEntitiesAsync<Student>((s => s.PID == id), cancellationToken: cancellationToken);
		}
	}
}
