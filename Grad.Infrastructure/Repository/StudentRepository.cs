using System.Security.Cryptography;
using Grad.Application.Common.Interfaces;
using Grad.Application.StudentFeatures.Interfaces;
using Grad.Domain.Enums;
using Grad.Domain.Model;
using Grad.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Grad.Infrastructure.Repository
{
	public class StudentRepository : Repository , IStudentRepository
	{
		Db_Context _context;
		IUowServices _UowServices;
		public StudentRepository(Db_Context context,IUowServices uow) : base(context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_UowServices = uow ?? throw new ArgumentNullException(nameof(uow));
		}

		public async Task<List<Enrollement>> GetEnrollementsAsync(Guid Sid,CancellationToken CT)
		{
			return	await _context.Set<Enrollement>().Where(e=>e.STUFK == Sid).Include(e=>e.studentProgresses).ToListAsync();
		}

		public async Task<bool> IsEnrolled(Guid Sid,Guid StdId,CancellationToken CT)
		{
			return await base.GetEntityAsync<Enrollement>(e => e.STUFK == StdId && e.SUBFK == Sid, CT) != null;
		}

		public async Task<int> EnrollSubject(Enrollement enrollement,CancellationToken CT)
		{
			base.CreateEntityAsync<Enrollement>(enrollement, CT);
			return await _UowServices.SaveChangesAsync();
		}

		public async Task<DisablityType> GetDisablityTypeAsync(Guid Sid,CancellationToken CT)
		{
			Student s = await base.GetEntityAsync<Student>(s=>s.Id ==  Sid,CT);
			return s != null ? s.Disability : DisablityType.None;
		}

		public async Task<Enrollement> GetEnrollementAsync(Guid SID, Guid STDID,CancellationToken CT = default)
		{
			return await _context.Set<Enrollement>().Where(e => e.STUFK == STDID && e.SUBFK == SID).Include(e => e.studentProgresses).FirstOrDefaultAsync();
		}
		//public async Task<List>
	}
}
