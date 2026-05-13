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

		public async Task<bool> isStudentExists(Guid sid,Guid pid,CancellationToken cancellationToken)
		{
			return await base.GetEntityAsync<Student>(s => s.PID == pid && s.Id == sid, cancellationToken: cancellationToken) != null;
		}
	}
}
